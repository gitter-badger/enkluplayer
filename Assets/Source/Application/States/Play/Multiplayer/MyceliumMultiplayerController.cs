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
using Enklu.Data;
using Enklu.Mycelium.Messages;
using Enklu.Mycerializer;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    public class SceneEventHandler
    {
        private readonly IAppSceneManager _scenes;
        private readonly int[] _nextChildIndices = new int[128];

        private Element _root;

        public SceneEventHandler(IAppSceneManager scenes)
        {
            _scenes = scenes;
        }

        public void Initialize()
        {
            if (_scenes.All.Length == 0)
            {
                Log.Error(this, "Tried to initialize SceneEventHandler but scene manager has no scenes!");
                return;
            }

            _root = _scenes.Root(_scenes.All[0]);
        }

        public void Uninitialize()
        {
            _root = null;
        }

        public void OnUpdated<T>(T evt) where T : UpdateElementEvent
        {
            if (null == _root)
            {
                return;
            }

            // find element
            var el = FindFast(_root, evt.ElementId);
            if (null == el)
            {
                Log.Warning(this, "Could not find element to update: {0}.", evt.ElementId);
                return;
            }

            var vec3 = evt as UpdateElementVec3Event;
            if (null != vec3)
            {
                el.Schema.Get<Vec3>(evt.PropName).Value = vec3.Value;
                return;
            }
            
            var fl = evt as UpdateElementFloatEvent;
            if (null != fl)
            {
                el.Schema.Get<float>(evt.PropName).Value = fl.Value;
                return;
            }

            var col4 = evt as UpdateElementCol4Event;
            if (null != col4)
            {
                el.Schema.Get<Col4>(evt.PropName).Value = col4.Value;
                return;
            }

            var it = evt as UpdateElementIntEvent;
            if (null != it)
            {
                el.Schema.Get<int>(evt.PropName).Value = it.Value;
                return;
            }

            var str = evt as UpdateElementStringEvent;
            if (null != str)
            {
                el.Schema.Get<string>(evt.PropName).Value = str.Value;
                return;
            }

            var bl = evt as UpdateElementBoolEvent;
            if (null != bl)
            {
                el.Schema.Get<bool>(evt.PropName).Value = bl.Value;
                return;
            }

            Log.Error(this, "Could not handle UpdateElementEvent {0}.", evt);
        }

        /// <summary>
        /// Stack-less search through hierarchy for an element that matches by
        /// id.
        /// </summary>
        /// <param name="root">Where to start the search.</param>
        /// <param name="id">The id to look for.</param>
        /// <returns></returns>
        private Element FindFast(Element root, string id)
        {
            var el = root;
            var compare = true;

            // prep indices
            var depthIndex = 0;
            _nextChildIndices[0] = 0;

            // search!
            while (true)
            {
                if (compare && el.Id == id)
                {
                    return el;
                }
                
                // get the index to the next child at this depth
                var nextChildIndex = _nextChildIndices[depthIndex];

                // proceed to next child
                if (nextChildIndex < el.Children.Count)
                {
                    // increment next child index at this depth
                    _nextChildIndices[depthIndex]++;

                    // get the next child
                    el = el.Children[nextChildIndex];

                    // move to the next depth
                    _nextChildIndices[++depthIndex] = 0;

                    // switch compare back on
                    compare = true;
                }
                // there is no next child
                else
                {
                    // move up a level
                    depthIndex--;

                    // there is nowhere else to go
                    if (depthIndex < 0)
                    {
                        return null;
                    }

                    // parent element
                    el = el.Parent;

                    // don't compare ids, we've already checked this element
                    compare = false;
                }
            }
        }
    }

    public class MyceliumMultiplayerController : IMultiplayerController, ISocketListener
    {
        private static int _Ids = 1000;
        
        private readonly IBootstrapper _bootstrapper;
        private readonly ApiController _api;
        private readonly ApplicationConfig _config;
        private readonly ReflectionMessageReader _reader = new ReflectionMessageReader();
        private readonly ReflectionMessageWriter _writer = new ReflectionMessageWriter();
        private readonly OptimizedObjectPool<ByteArrayHandle> _buffers = new OptimizedObjectPool<ByteArrayHandle>(
            4, 0, 1,
            () => new ByteArrayHandle(1024));
        private readonly List<UpdateElementEvent> _sceneEventBuffer = new List<UpdateElementEvent>();
        private readonly SceneEventHandler _sceneHandler;

        private readonly Dictionary<Type, Delegate> _subscriptions = new Dictionary<Type, Delegate>();
        private readonly List<object> _eventWaitBuffer = new List<object>();
        private object[] _eventExecutionBuffer = new object[4];
        private int _eventExecutionBufferLen;

        private TcpConnection _tcp;
        private AsyncToken<Void> _connect;
        private string _multiplayerToken;
        private int _pollId;

        public MyceliumMultiplayerController(
            IBootstrapper bootstrapper,
            IAppSceneManager scenes,
            ApiController api,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _api = api;
            _config = config;
            _sceneHandler = new SceneEventHandler(scenes);
        }

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

        public void ApplyDiff(IAppDataLoader appData)
        {
            // TODO: apply diff
        }

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
        
        public void Send(object message)
        {
            Write(message);
        }

        public void Subscribe<T>(Action<T> callback)
        {
            _subscriptions[typeof(T)] = callback;
        }

        public void UnSubscribe<T>()
        {
            _subscriptions.Remove(typeof(T));
        }

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
                            _multiplayerToken = res.Payload.Body;

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

                // before we login, make sure we're listening for events
                Subscribe<UpdateElementVec3Event>(OnBufferEvent);
                Subscribe<UpdateElementCol4Event>(OnBufferEvent);
                Subscribe<UpdateElementIntEvent>(OnBufferEvent);
                Subscribe<UpdateElementFloatEvent>(OnBufferEvent);
                Subscribe<UpdateElementStringEvent>(OnBufferEvent);
                Subscribe<UpdateElementBoolEvent>(OnBufferEvent);

                // next, send login request
                Log.Info(this, "Sending login request.");
                Subscribe<LoginResponse>(OnLoginResponse);
                Send(new LoginRequest
                {
                    Jwt = _multiplayerToken
                });
            }
            else
            {
                _connect.Fail(new Exception("Could not connect to mycelium."));
            }
        }
        
        private void StartPoll()
        {
            lock (_eventWaitBuffer)
            {
                _eventWaitBuffer.Clear();
            }

            _bootstrapper.BootstrapCoroutine(Poll());
        }

        private IEnumerator Poll()
        {
            var id = _pollId = _Ids++;

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

        private void StopPoll()
        {
            _pollId = -1;
        }

        private void OnBufferEvent(UpdateElementEvent evt)
        {
            _sceneEventBuffer.Add(evt);
        }

        private void OnLoginResponse(LoginResponse res)
        {
            // w00t
            Log.Info(this, "Logged into Mycelium successfully!");

            _connect.Succeed(Void.Instance);
        }
        
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

        private void Write(object message)
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

        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(object message, params object[] replacements)
        {
            Log.Debug(this, message, replacements);
        }
    }
}