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
        /// Buffers events until we're fully connected.
        /// </summary>
        private readonly List<UpdateElementEvent> _sceneEventBuffer = new List<UpdateElementEvent>();

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
        /// Buffer that read thread pushes messages onto. Main thread polls for
        /// messages to execute on main thread.
        /// </summary>
        private readonly List<object> _eventWaitBuffer = new List<object>();

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
        private TcpConnection _tcp;

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
        /// Constructor.
        /// </summary>
        public MyceliumMultiplayerController(
            IBootstrapper bootstrapper,
            IElementManager elements,
            IAppSceneManager scenes,
            IElementActionStrategyFactory patcherFactory,
            ApiController api,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _elements = elements;
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

            _connect = new AsyncToken<Void>();

            Log.Info(this, "Requesting multiplayer access token.");

            // start polling the message queue
            StartPoll();

            // first, request  multiplayer token
            GetMultiplayerToken();

            return _connect.Token();
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

                // stop buffering events and forward them to the scene handler
                Subscribe<UpdateElementVec3Event>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementCol4Event>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementIntEvent>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementFloatEvent>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementStringEvent>(_sceneHandler.OnUpdated);
                Subscribe<UpdateElementBoolEvent>(_sceneHandler.OnUpdated);

                // forward buffered events to the scene handler
                for (int i = 0, len = _sceneEventBuffer.Count; i < len; i++)
                {
                    _sceneHandler.OnUpdated(_sceneEventBuffer[i]);
                }
            }
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
            _sceneHandler.Initialize();

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
                }
            }

            // if the request could not be sent, set a timer to flip it back
            if (!reqSent)
            {
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
            lock (_eventWaitBuffer)
            {
                _eventWaitBuffer.Add(message);
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
                _config.Network.Environment.MyceliumUrl,
                _config.Network.Environment.MyceliumPort);

            _tcp = new TcpConnection(
                new LengthBasedSocketMessageReader(this),
                new LengthBasedSocketMessageWriter());
            if (_tcp.Connect(
                _config.Network.Environment.MyceliumUrl,
                _config.Network.Environment.MyceliumPort))
            {
                Log.Info(this, "Connected to mycelium.");

                // before we login, make sure we're buffering events
                Subscribe<UpdateElementVec3Event>(OnBufferEvent);
                Subscribe<UpdateElementCol4Event>(OnBufferEvent);
                Subscribe<UpdateElementIntEvent>(OnBufferEvent);
                Subscribe<UpdateElementFloatEvent>(OnBufferEvent);
                Subscribe<UpdateElementStringEvent>(OnBufferEvent);
                Subscribe<UpdateElementBoolEvent>(OnBufferEvent);

                // ... and listen for the scene diff
                Subscribe<SceneDiffEvent>(_sceneHandler.OnDiff);

                // ... and listen to map updates
                Subscribe<SceneMapUpdateEvent>(_sceneHandler.OnMapUpdated);

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
            lock (_eventWaitBuffer)
            {
                _eventWaitBuffer.Clear();
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
                lock (_eventWaitBuffer)
                {
                    _eventExecutionBufferLen = _eventWaitBuffer.Count;

                    // grow
                    while (_eventExecutionBuffer.Length < _eventExecutionBufferLen)
                    {
                        _eventExecutionBuffer = new object[_eventExecutionBuffer.Length * 2];
                    }

                    // copy
                    for (var i = 0; i < _eventExecutionBufferLen; i++)
                    {
                        _eventExecutionBuffer[i] = _eventWaitBuffer[i];
                    }

                    _eventWaitBuffer.Clear();
                }

                // handle events
                for (var i = 0; i < _eventExecutionBufferLen; i++)
                {
                    Handle(_eventExecutionBuffer[i]);
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
        /// Called when an message is received but we are not ready for it yet, so we buffer it.
        /// </summary>
        /// <param name="evt"></param>
        private void OnBufferEvent(UpdateElementEvent evt)
        {
            _sceneEventBuffer.Add(evt);
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
        /// Handles a message.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        private void Handle(object message)
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