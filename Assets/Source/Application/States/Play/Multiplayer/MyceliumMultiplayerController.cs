using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.DataStructures;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.CreateMultiplayerToken;
using Enklu.Data;
using Enklu.Mycelium.Messages;
using Enklu.Mycelium.Messages.Experience;
using Enklu.Mycerializer;
using UnityEngine;
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
        private readonly IMessageRouter _messageRouter;
        private readonly ApiController _api;
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Reads from streams.
        /// </summary>
        private readonly IMessageReader _reader;

        /// <summary>
        /// Writes to streams.
        /// </summary>
        private readonly IMessageWriter _writer;

        /// <summary>
        /// Keeps track of requests that are out.
        /// </summary>
        private readonly Dictionary<int, Action<object>> _requestMap = new Dictionary<int, Action<object>>();

        /// <summary>
        /// Byte buffers.
        /// </summary>
        private readonly OptimizedObjectPool<ByteArrayHandle> _buffers = new OptimizedObjectPool<ByteArrayHandle>(
            4, 0, 1,
            () => new ByteArrayHandle(1024));

        /// <summary>
        /// Back off calculation that occurs each reconnect attempt. Provides a nice spacing between
        /// attempts preventing spam when the server isn't available.
        /// </summary>
        private readonly IBackOff _backOff = new ExponentialBackOff(5.0, 30.0);
        
        /// <summary>
        /// Handles scene events.
        /// </summary>
        private readonly SceneEventHandler _sceneHandler;

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
        private bool _needsReconnect = false;

        /// <summary>
        /// Flag to denote we're either reconnecting or waiting to reconnect.
        /// </summary>
        private bool _isReconnecting = false;

        /// <summary>
        /// True iff we are logged in successfully.
        /// </summary>
        private bool _loggedIn;

        /// <inheritdoc />
        public bool IsConnected
        {
            get { return null != Tcp && Tcp.IsConnected; }
        }

        /// <inheritdoc />
        public event Action<bool> OnConnectionChanged;

        /// <summary>
        /// Retrieves the underlying tcp connection.
        /// </summary>
        public ITcpConnection Tcp { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MyceliumMultiplayerController(
            IBootstrapper bootstrapper,
            IElementManager elements,
            IElementFactory elementFactory,
            IAppSceneManager scenes,
            IElementActionStrategyFactory patcherFactory,
            ITcpConnectionFactory connections,
            IMessageRouter messageRouter,
            IMessageReader reader,
            IMessageWriter writer,
            ApiController api,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _elements = elements;
            _connections = connections;
            _messageRouter = messageRouter;
            _reader = reader;
            _writer = writer;
            _api = api;
            _config = config;

            _sceneHandler = new SceneEventHandler(
                _elements,
                elementFactory,
                new ScenePatcher(scenes, patcherFactory));
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Initialize()
        {
            if (null != _connect)
            {
                return _connect.Token();
            }

            // Add OnResume subscriber to handle reconnects
            _messageRouter.Subscribe(MessageTypes.APPLICATION_RESUME, OnApplicationResume);

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
                Subscribe<CreateElementEvent>(evt => _sceneHandler.OnCreated(evt));
                Subscribe<DeleteElementEvent>(_sceneHandler.OnDeleted);
                Subscribe<SceneDiffEvent>(_sceneHandler.OnDiff);
                Subscribe<SceneMapUpdateEvent>(_sceneHandler.OnMapUpdated);
                Subscribe<CreateElementResponse>(OnResponse);
                Subscribe<DeleteElementResponse>(OnResponse);

                // process buffered events
                StopQueueingMessages();
            }
        }
        
        /// <inheritdoc />
        public void Disconnect()
        {
            if (null == _connect)
            {
                return;
            }

            StopPoll();

            if (null != Tcp)
            {
                Tcp.OnConnectionClosed -= OnConnectionClosed;
                try
                {
                    Tcp.Close();

                    if (null != OnConnectionChanged)
                    {
                        OnConnectionChanged(false);
                    }
                }
                catch (Exception e)
                {
                    Log.Debug("Socket Close thew harmless exception on disconnect: {0}", e);
                }
                Tcp = null;
            }

            _connect.Fail(new Exception("Disconnected."));
            _connect = null;

            // unsubscribe everything
            _subscriptions.Clear();

            // kill scene handler
            _sceneHandler.Uninitialize();
        }
        
        public IAsyncToken<Element> Create(
            string parentId,
            ElementData element,
            string owner = null,
            ElementExpirationType expiration = ElementExpirationType.Session)
        {
            var token = new AsyncToken<Element>();

            var parentHash = _sceneHandler.ElementHash(parentId);
            var req = new CreateElementRequest
            {
                ParentHash = parentHash,
                Element = element,
                Owner = owner,
                Expiration = expiration
            };

            _requestMap[req.RequestId] = obj =>
            {
                var res = (CreateElementResponse) obj;
                if (res.Success)
                {
                    // send to scene handler
                    var newElement = _sceneHandler.OnCreated(new CreateElementEvent
                    {
                        Element = element,
                        ParentHash = parentHash
                    });

                    // resolve token
                    token.Succeed(newElement);
                }
                else
                {
                    token.Fail(new Exception("Could not create element."));
                }
            };

            Send(req);
            
            return token;
        }

        public IAsyncToken<Void> Destroy(string id)
        {
            var token = new AsyncToken<Void>();

            var req = new DeleteElementRequest
            {
                ElementHash = _sceneHandler.ElementHash(id)
            };

            _requestMap[req.RequestId] = obj =>
            {
                var res = (DeleteElementResponse) obj;
                if (res.Success)
                {
                    token.Succeed(Void.Instance);
                }
                else
                {
                    token.Fail(new Exception("Could not delete element."));
                }
            };

            Send(req);

            return token;
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

            // set a timer to flip back
            _bootstrapper.BootstrapCoroutine(Wait(
                milliseconds / 1000f,
                () => element.Schema.Set(prop, !value)));

            if (null == Tcp)
            {
                return;
            }

            // try to send a request
            if (Tcp.IsConnected)
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
                    Send(new AutoToggleEvent
                    {
                        ElementHash = hash,
                        PropName = prop,
                        Milliseconds = milliseconds,
                        StartingValue = value
                    });
                }
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
            lock (_synchronizationBuffer)
            {
                _synchronizationBuffer.Clear();
            }

            // Cleanup Tcp Connection
            if (null != Tcp)
            {
                // Remove listener and explicit
                Tcp.OnConnectionClosed -= OnConnectionClosed;
                try
                {
                    Tcp.Close();
                }
                catch (Exception e)
                {
                    // This is not indicative of an actual problem, but ensures we we flush buffers,
                    // and cleanup threads
                    Verbose("Caught exception during force close on TcpConnection: {0}", e);
                }

                Tcp = null;
            }

            if (null != _connect)
            {
                _connect.Abort();
                _connect = null;
            }

            return Connect();
        }

        /// <summary>
        /// Handle disconnection from server.
        /// </summary>
        private void OnConnectionClosed(bool resetByPeer)
        {
            Log.Debug(this, "OnConnectionClosed[ResetByPeer: {0}]", resetByPeer);

            // Flip flag and allow Poll() to handle
            _needsReconnect = true;

            if (null != OnConnectionChanged)
            {
                OnConnectionChanged(false);
            }
        }

        /// <summary>
        /// When the application resumes, we always assume a disconnected state and reconnect.
        /// </summary>
        private void OnApplicationResume(object obj)
        {
            Log.Warning(this, "OnApplicationResume!");

            _needsReconnect = true;
        }
        
        /// <summary>
        /// Called when a delete response is received.
        /// </summary>
        /// <param name="response">The response.</param>
        private void OnResponse(RoomResponse response)
        {
            Action<object> callback;
            if (!_requestMap.TryGetValue(response.RequestId, out callback))
            {
                Log.Warning(this, "Received a {0} for an unknown request id: {1}.",
                    response.GetType().Name,
                    response.RequestId);
                return;
            }

            _requestMap.Remove(response.RequestId);

            callback(response);
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
                            Log.Warning(this, "Could not connect to trellis for multiplayer token. Succeeding token and using local fallback.");

                            if (!_isReconnecting)
                            {
                                _needsReconnect = true;
                            }

                            _connect.Succeed(Void.Instance);
                        }
                    }
                    else
                    {
                        Log.Warning(this, "Could not connect to trellis for multiplayer token. Succeeding token and using local fallback.");

                        if (!_isReconnecting)
                        {
                            _needsReconnect = true;
                        }

                        _connect.Succeed(Void.Instance);
                    }
                })
                .OnFailure(ex =>
                {
                    Log.Warning(this, "Could not connect to trellis for multiplayer token. Succeeding token and using local fallback.");

                    if (!_isReconnecting)
                    {
                        _needsReconnect = true;
                    }

                    _connect.Succeed(Void.Instance);
                });
        }

        /// <summary>
        /// Connects to Mycelium.
        /// </summary>
        private void ConnectToMycelium()
        {
            Log.Info(this, "Connecting to mycelium at {0}:{1}.",
                _config.Network.Environment.MyceliumIp,
                _config.Network.Environment.MyceliumPort);

            Tcp = _connections.Connection(this);
            if (Tcp.Connect(
                _config.Network.Environment.MyceliumIp,
                _config.Network.Environment.MyceliumPort))
            {
                Log.Info(this, "Connected to mycelium.");

                if (null != OnConnectionChanged)
                {
                    OnConnectionChanged(true);
                }

                Tcp.OnConnectionClosed += OnConnectionClosed;

                // next, send login request
                _bootstrapper.BootstrapCoroutine(Login());
            }
            else
            {
                Log.Warning(this, "Could not connect to mycelium. Succeeding token and using local fallback.");

                if (!_isReconnecting)
                {
                    _needsReconnect = true;
                }

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
                if (_needsReconnect)
                {
                    // Reset Flag, Reconnect
                    _needsReconnect = false;

                    // Flag Reconnect
                    _isReconnecting = true;

                    Reconnect()
                        .OnSuccess(_ =>
                        {
                            // Success for Connect always happens to prevent outside
                            // systems from failing. Internally, we can determine
                            // success vs failure if the TcpConnection is reporting
                            // IsConnected
                            if (IsConnected)
                            {
                                Log.Debug(this, "Reconnect Successful");

                                // Reset Connection Back Off
                                _backOff.Reset();

                                return;
                            }

                            // Otherwise, we'll flip our flag back and retry after a timeout
                            var nextRetryTime = (float) _backOff.Next();
                            Log.Debug(this, "Reconnect Failed; Trying again in: {0} seconds", nextRetryTime);

                            _bootstrapper.BootstrapCoroutine(
                                Wait(nextRetryTime, () => _needsReconnect = true));
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
            _loggedIn = false;

            // TODO: this is here due to a mycelium bug
            yield return new WaitForSeconds(0.5f);

            Log.Info(this, "Sending login request.");

            Subscribe<LoginResponse>(OnLoginResponse);
            Send(new LoginRequest
            {
                Jwt = _multiplayerJwt
            });

            // timeout
            yield return new WaitForSeconds(3f);

            if (!_loggedIn)
            {
                Log.Info(this, "Login timed out. Proceeding anyway.");

                if (!_isReconnecting)
                {
                    _needsReconnect = true;
                }

                _connect.Succeed(Void.Instance);
            }
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

            _loggedIn = true;
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
            if (null == Tcp || !Tcp.IsConnected)
            {
                return;
            }

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

            try
            {
                // write type first
                stream.WriteUnsignedShort(id);

                // write object
                _writer.Write(message, stream);

                Verbose("Wrote {0} bytes.", stream.WriterIndex);

                Tcp.Send(buffer.Buffer, 0, stream.WriterIndex);
            }
            finally
            {
                // return buffer
                _buffers.Put(buffer);
            }
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
