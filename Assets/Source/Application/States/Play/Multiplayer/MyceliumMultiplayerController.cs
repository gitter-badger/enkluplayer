using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.DataStructures;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.CreateMultiplayerToken;
using Enklu.Mycelium.Messages;
using Enklu.Mycerializer;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
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
            ApiController api,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _api = api;
            _config = config;
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
            // TODO: stop buffering events

            // TODO: apply event buffer
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