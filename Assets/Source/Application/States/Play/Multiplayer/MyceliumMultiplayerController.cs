using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.DataStructures;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.CreateMultiplayerToken;
using Enklu.Mycelium.Messages;
using Enklu.Mycelium.Messages.Experience;
using Enklu.Mycerializer;
using UnityEngine;
using Enklu.Data;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// <c>IMultiplayerController</c> implementation that connects to Mycelium.
    /// </summary>
    public class MyceliumMultiplayerController : IMultiplayerController, ISocketListener
    {
        /// <summary>
        /// Id generator for polls.
        /// </summary>
        private static int _PollIds = 1000;

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        private readonly IElementManager _elements;
        private readonly ITcpConnectionFactory _connections;
        private readonly ApiController _api;
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Reads from streams.
        /// </summary>
        private readonly ReflectionMessageReader _reader = new ReflectionMessageReader();

        /// <summary>
        /// Writes to streams.
        /// </summary>
        private readonly ReflectionMessageWriter _writer = new ReflectionMessageWriter();

        /// <summary>
        /// Byte buffers.
        /// </summary>
        private readonly OptimizedObjectPool<ByteArrayHandle> _buffers = new OptimizedObjectPool<ByteArrayHandle>(
            4, 0, 1,
            () => new ByteArrayHandle(1024));

        /// <summary>
        /// Handles scene events.
        /// </summary>
        private readonly SceneEventHandler _sceneHandler;

        /// <summary>
        /// Handles patching the current scene with diffs created in a multiplayer environment.
        /// </summary>
        private readonly ScenePatcher _scenePatcher;

        /// <summary>
        /// Maps message type to the delegate that should handle it.
        /// </summary>
        private readonly Dictionary<Type, Delegate> _subscriptions = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Buffers events until we're fully connected.
        /// </summary>
        private readonly List<object> _eventQueue = new List<object>();

        /// <summary>
        /// Buffer that read thread pushes messages onto. Main thread polls for
        /// messages to execute on main thread.
        /// </summary>
        private readonly List<object> _synchronizationBuffer = new List<object>();

        /// <summary>
        /// True iff we are queueing rather than dispatching to subscriptions.
        /// </summary>
        private bool _isQueueingMessages = false;

        /// <summary>
        /// The main thread copies events into this buffer from _eventWaitBuffer.
        /// </summary>
        private object[] _eventExecutionBuffer = new object[4];

        /// <summary>
        /// Keeps track of how many events are in the execution buffer.
        /// </summary>
        private int _eventExecutionBufferLen;

        /// <summary>
        /// The underlying TCP connection.
        /// </summary>
        private ITcpConnection _tcp;

        /// <summary>
        /// Internal connection token.
        /// </summary>
        private AsyncToken<Void> _connect;

        /// <summary>
        /// The JWT returned to connect mycelium.
        /// </summary>
        private string _multiplayerJwt;

        /// <summary>
        /// Id of the current poll.
        /// </summary>
        private int _pollId;

        /// <summary>
        /// Flag whether or not the connection has been closed.
        /// </summary>
        private bool _isConnectionClosed = false;

        /// <inheritdoc />
        public bool IsConnected
        {
            get { return null != _tcp && _tcp.IsConnected; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MyceliumMultiplayerController(
            IBootstrapper bootstrapper,
            IElementManager elements,
            IAppSceneManager scenes,
            IElementActionStrategyFactory patcherFactory,
            ITcpConnectionFactory connections,
            ApiController api,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _elements = elements;
            _connections = connections;
            _api = api;
            _config = config;

            _scenePatcher = new ScenePatcher(scenes, patcherFactory);
            _sceneHandler = new SceneEventHandler(_elements, _scenePatcher);
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Initialize()
        {
            if (null != _connect)
            {
                return _connect.Token();
            }

            // make sure we are buffering events
            StartQueueingMessages();

            // start polling the message queue
            StartPoll();

            return Connect();
        }

        /// <inheritdoc />
        public void ApplyDiff(IAppDataLoader appData)
        {
            // _scenePatcher.ApplyTo() ?
        }

        /// <inheritdoc />
        public void Play()
        {
            // prep the scene handler
            {
                _sceneHandler.Initialize();

                // forward events to scene handler
                Subscribe<UpdateElementVec3Event>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementCol4Event>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementIntEvent>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementFloatEvent>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementStringEvent>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementBoolEvent>(_sceneHandler.OnUpdated);
                Subscribe<SceneDiffEvent>(_sceneHandler.OnDiff);
                Subscribe<SceneMapUpdateEvent>(_sceneHandler.OnMapUpdated);

                // process buffered events
                StopQueueingMessages();
            }
        }

        /// <summary>
        /// Gets a multiplayer token and connects to mycelium.
        /// </summary>
        private IAsyncToken<Void> Connect()
        {
            if (null != _connect)
            {
                return _connect.Token();
            }

            _connect = new AsyncToken<Void>();

            Log.Info(this, "Requesting multiplayer access token.");
            GetMultiplayerToken();

            return _connect.Token();
        }

        /// <summary>
        /// Reacquire multiplayer token and reconnect to mycelium
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> Reconnect()
        {
            // Cleanup Tcp Connection
            if (null != _tcp)
            {
                // Remove listener and explicit
                _tcp.OnConnectionClosed -= OnConnectionClosed;
                try
                {
                    _tcp.Close();
                }
                catch (Exception e)
                {
                    // This is not indicative of an actual problem, but ensures we we flush buffers,
                    // and cleanup threads
                    Verbose("Caught exception during force close on TcpConnection: {0}", e);
                }

                _tcp = null;
            }

            // Reset Connection token
            if (null != _connect)
            {
                // This case might be possible. Just ensure we pass the error along.
                // Or we could Abort()?
                _connect.Fail(new Exception("Attempted Reconnection during Connect."));
            }
            _connect = null;

            return Connect();
        }

        /// <summary>
        /// Handle disconnection from server.
        /// </summary>
        private void OnConnectionClosed(bool resetByPeer)
        {
            Log.Debug(this, "OnConnectionClosed[ResetByPeer: {0}]", resetByPeer);

            // Flip flag and allow Poll() to handle
            _isConnectionClosed = true;
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            if (null == _connect)
            {
                return;
            }

            _tcp.Close();
            _tcp = null;

            _connect.Fail(new Exception("Disconnected."));
            _connect = null;

            // unsubscribe everything
            _subscriptions.Clear();

            // kill scene handler
            _sceneHandler.Uninitialize();

            StopPoll();
        }

        /// <inheritdoc />
        public void Sync(ElementSchemaProp prop)
        {
            Log.Warning(this, "Sync not implemented.");
        }

        /// <inheritdoc />
        public void UnSync(ElementSchemaProp prop)
        {
            Log.Warning(this, "UnSync not implemented.");
        }

        /// <inheritdoc />
        public void Own(string elementId, Action<bool> callback)
        {
            Log.Warning(this, "Own not implemented.");

            callback(false);
        }

        /// <inheritdoc />
        public void Forfeit(string elementId)
        {
            Log.Warning(this, "Forfeit not implemented.");
        }

        /// <inheritdoc />
        public void AutoToggle(string elementId, string prop, bool value, int milliseconds)
        {
            // find element to apply to
            var element = _elements.ById(elementId);
            if (null == element)
            {
                Log.Warning(this, "Could not find element by id {0} to autotoggle.", elementId);
                return;
            }

            // set prop locally
            element.Schema.Set(prop, value);

            // first, try to send a request
            var reqSent = false;
            if (_tcp.IsConnected)
            {
                // we need the hash to send a request
                var hash = _sceneHandler.ElementHash(elementId);

                // no hash found
                if (0 == hash)
                {
                    Log.Warning(this, "Cound not find hash for element '{0}'.", elementId);
                }
                // hash found, send request
                else
                {
                    reqSent = true;

                    Send(new AutoToggleEvent
                    {
                        ElementHash = hash,
                        PropName = prop,
                        Milliseconds = milliseconds,
                        StartingValue = value
                    });

                    // TODO: set a timer
                }
            }

            // if the request could not be sent, set a timer to flip it back
            if (!reqSent)
            {
                Log.Info(this, "Mycelium is not connected: Use local AutoToggle.");

                _bootstrapper.BootstrapCoroutine(Wait(
                    milliseconds / 1000f,
                    () => element.Schema.Set(prop, !value)));
            }
        }

        /// <inheritdoc />
        public void HandleSocketMessage(ArraySegment<byte> bytes)
        {
            var stream = new ByteStream(bytes.Array, bytes.Offset);

            // read type
            var id = stream.ReadUnsignedShort();
            Type type;
            try
            {
                type = MyceliumMessagesMap.Get(id);
            }
            catch
            {
                Log.Error(this, "Unknown message type {0}.", id);
                return;
            }

            object message;
            try
            {
                message = _reader.Read(type, stream);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not read from stream: {0}.", exception);
                return;
            }

            // push to main thread
            lock (_synchronizationBuffer)
            {
                _synchronizationBuffer.Add(message);
            }
        }

        /// <summary>
        /// Subscribes to a message.
        /// </summary>
        /// <typeparam name="T">The type parameter.</typeparam>
        /// <param name="callback">The callback.</param>
        private void Subscribe<T>(Action<T> callback)
        {
            _subscriptions[typeof(T)] = callback;
        }

        /// <summary>
        /// Attempts to retrieve a multiplayer token.
        /// </summary>
        private void GetMultiplayerToken()
        {
            // ask trellis
            _api
                .PublishedApps
                .CreateMultiplayerToken(_config.Play.AppId, new Request())
                .OnSuccess(res =>
                {
                    if (null != res.Payload)
                    {
                        if (res.Payload.Success)
                        {
                            _multiplayerJwt = res.Payload.Body;

                            Log.Info(this, "Successfully received multiplayer access token.");

                            ConnectToMycelium();
                        }
                        else
                        {
                            _connect.Fail(new Exception(string.Format(
                                "Could not create multiplayer token: {0}.", res.Payload.Error)));
                        }
                    }
                    else
                    {
                        _connect.Fail(new Exception(string.Format(
                            "Could not create multiplayer token: {0}.", Encoding.UTF8.GetString(res.Raw))));
                    }
                })
                .OnFailure(_connect.Fail);
        }

        /// <summary>
        /// Connects to Mycelium.
        /// </summary>
        private void ConnectToMycelium()
        {
            Log.Info(this, "Connecting to mycelium at {0}:{1}.",
                _config.Network.Environment.MyceliumIp,
                _config.Network.Environment.MyceliumPort);

            _tcp = _connections.Connection(this);
            if (_tcp.Connect(
                _config.Network.Environment.MyceliumIp,
                _config.Network.Environment.MyceliumPort))
            {
                Log.Info(this, "Connected to mycelium.");

                _tcp.OnConnectionClosed += OnConnectionClosed;

                // next, send login request
                _bootstrapper.BootstrapCoroutine(Login());
            }
            else
            {
                Log.Error(this, "Could not connect to mycelium. Succeeding token and using local fallback.");

                _connect.Succeed(Void.Instance);
            }
        }

        /// <summary>
        /// Starts polling for events.
        /// </summary>
        private void StartPoll()
        {
            lock (_synchronizationBuffer)
            {
                _synchronizationBuffer.Clear();
            }

            _bootstrapper.BootstrapCoroutine(Poll());
        }

        /// <summary>
        /// Polls for events in the buffer.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Poll()
        {
            var id = _pollId = _PollIds++;

            while (id == _pollId)
            {
                yield return null;

                // copy events
                lock (_synchronizationBuffer)
                {
                    _eventExecutionBufferLen = _synchronizationBuffer.Count;

                    // grow
                    while (_eventExecutionBuffer.Length < _eventExecutionBufferLen)
                    {
                        _eventExecutionBuffer = new object[_eventExecutionBuffer.Length * 2];
                    }

                    // copy
                    for (var i = 0; i < _eventExecutionBufferLen; i++)
                    {
                        _eventExecutionBuffer[i] = _synchronizationBuffer[i];
                    }

                    _synchronizationBuffer.Clear();
                }

                // handle events
                for (var i = 0; i < _eventExecutionBufferLen; i++)
                {
                    Handle(_eventExecutionBuffer[i]);
                }

                // check for flagged connection close
                if (_isConnectionClosed)
                {
                    // Reset Flag, Reconnect
                    _isConnectionClosed = false;
                    Reconnect()
                        .OnSuccess(_ =>
                        {
                            Log.Debug(this, "Reconnect Successful");
                        })
                        .OnFailure(e =>
                        {
                            Log.Debug(this, "Reconnection Failed: {0}", e);
                        });
                }
            }
        }

        /// <summary>
        /// Logs in.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Login()
        {
            // TODO: this is here due to a mycelium bug
            yield return new WaitForSeconds(0.5f);

            Log.Info(this, "Sending login request.");

            Subscribe<LoginResponse>(OnLoginResponse);
            Send(new LoginRequest
            {
                Jwt = _multiplayerJwt
            });
        }

        /// <summary>
        /// Waits for a time before executing the action on the main thread.
        /// </summary>
        /// <param name="seconds">The number of seconds to wait.</param>
        /// <param name="action">The action to call.</param>
        /// <returns></returns>
        private IEnumerator Wait(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);

            action();
        }

        /// <summary>
        /// Stops polling.
        /// </summary>
        private void StopPoll()
        {
            _pollId = -1;
        }

        /// <summary>
        /// Called when we receive a response from login.
        /// </summary>
        /// <param name="res">The response.</param>
        private void OnLoginResponse(LoginResponse res)
        {
            // w00t
            Log.Info(this, "Logged into Mycelium successfully!");

            _connect.Succeed(Void.Instance);
        }

        /// <summary>
        /// Queues messages.
        /// </summary>
        private void StartQueueingMessages()
        {
            _isQueueingMessages = true;
        }

        /// <summary>
        /// Stops queueing messages and dispatches all queued messages.
        /// </summary>
        private void StopQueueingMessages()
        {
            if (!_isQueueingMessages)
            {
                return;
            }

            _isQueueingMessages = false;

            var copy = _eventQueue.ToArray();
            _eventQueue.Clear();

            for (int i = 0, len = copy.Length; i < len; i++)
            {
                Dispatch(copy[i]);
            }
        }

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        private void Handle(object message)
        {
            // login can sneak by queue
            if (!(message is LoginResponse) && _isQueueingMessages)
            {
                _eventQueue.Add(message);

                return;
            }

            Dispatch(message);
        }

        /// <summary>
        /// Dispatches event to handler.
        /// </summary>
        /// <param name="message">The message</param>
        private void Dispatch(object message)
        {
            var type = message.GetType();
            Delegate handler;
            if (!_subscriptions.TryGetValue(type, out handler))
            {
                Log.Warning(this, "No handler for {0}.", message);
            }
            else
            {
                Verbose("Handling a {0}.", message);

                try
                {
                    handler.DynamicInvoke(message);
                }
                catch (Exception exception)
                {
                    Log.Error(this, "Could not invoke handler for {0} : {1}.",
                        type,
                        exception);
                }
            }
        }

        /// <summary>
        /// Sends a message to Mycelium.
        /// </summary>
        /// <param name="message">The message to send.</param>
        private void Send(object message)
        {
            Verbose("Sending {0}.", message);

            ushort id;
            try
            {
                id = MyceliumMessagesMap.Get(message.GetType());
            }
            catch
            {
                Log.Error(this, "Cannot send message that is not in messages map.");
                return;
            }

            var buffer = _buffers.Get();
            var stream = new ByteStream(buffer);

            // write type first
            stream.WriteUnsignedShort(id);

            // write object
            _writer.Write(message, stream);

            Verbose("Wrote {0} bytes.", stream.WriterIndex);

            _tcp.Send(buffer.Buffer, 0, stream.WriterIndex);

            // return buffer
            _buffers.Put(buffer);
        }

        /// <summary>
        /// Verbose logging method.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(object message, params object[] replacements)
        {
            Log.Debug(this, message, replacements);
        }
    }
}