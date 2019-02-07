using System;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.DataStructures;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.CreateMultiplayerToken;
using Enklu.Mycelium.Messages;
using Enklu.Mycerializer;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    public class MyceliumMultiplayerController
    {
        private readonly ApiController _api;
        private readonly ApplicationConfig _config;
        private readonly ReflectionMessageReader _reader = new ReflectionMessageReader();
        private readonly ReflectionMessageWriter _writer = new ReflectionMessageWriter();
        private readonly OptimizedObjectPool<ByteArrayHandle> _buffers = new OptimizedObjectPool<ByteArrayHandle>(
            4, 0, 1,
            () => new ByteArrayHandle(1024));

        private TcpConnection _tcp;
        private AsyncToken<Void> _connect;
        private string _multiplayerToken;
        
        public MyceliumMultiplayerController(
            ApiController api,
            ApplicationConfig config)
        {
            _api = api;
            _config = config;
        }

        public IAsyncToken<Void> Connect()
        {
            if (null != _connect)
            {
                return _connect.Token();
            }

            _connect = new AsyncToken<Void>();

            // first, request  multiplayer token
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

            return _connect.Token();
        }

        public void Disconnect()
        {
            if (null != _connect)
            {
                _tcp.Close();
                _tcp = null;

                _connect.Fail(new Exception("Disconnected."));
                _connect = null;
            }
        }

        private void ConnectToMycelium()
        {
            Log.Info(this, "Connecting to mycelium at {0}:{1}.",
                _config.Network.Environment.MyceliumUrl,
                _config.Network.Environment.MyceliumPort);

            _tcp = new TcpConnection(
                new LengthBasedSocketMessageReader(2, Read),
                new LengthBasedSocketMessageWriter(2));
            if (_tcp.Connect(
                _config.Network.Environment.MyceliumUrl,
                _config.Network.Environment.MyceliumPort))
            {
                Log.Info(this, "Connected to mycelium.");

                // next, send login request
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

        public void Send(LoginRequest req)
        {
            Write(req);
        }

        public void Subscribe<T>(Action<T> callback)
        {
            
        }

        public void UnSubscribe<T>()
        {

        }

        private void OnLoginResponse(LoginResponse res)
        {
            // w00t
            Log.Info(this, "Logged into Mycelium successfully.");

            _connect.Succeed(Void.Instance);
        }
        
        private void Read(ArraySegment<byte> bytes)
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

            Log.Info(this, "Received a {0}.", message);
        }

        private void Write(object message)
        {
            var buffer = _buffers.Get();
            var stream = new ByteStream(buffer);

            _writer.Write(message, stream);
            _tcp.Send(buffer.Buffer, 0, stream.WriterIndex);
        }
    }
}
