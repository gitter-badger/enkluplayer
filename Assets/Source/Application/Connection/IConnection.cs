using System;
using System.Runtime.Remoting;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using LightJson;
using WebSocketSharp;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public interface IConnection
    {
        IAsyncToken<Void> Connect(ApplicationConfig config);
    }

    public class WebSocketRequestData
    {
        public class HeaderData
        {
            public string Authorization;
        }
        
        [JsonName("url")]
        public string Url;

        [JsonName("method")]
        public string Method;

        [JsonName("headers")]
        public HeaderData Headers;

        [JsonName("data")]
        public object Data;
    }

    public class WebSocketSharpConnection : IConnection
    {
        private readonly JsonSerializer _json = new JsonSerializer();

        private ApplicationConfig _config;
        private WebSocket _socket;

        public IAsyncToken<Void> Connect(ApplicationConfig config)
        {
            var token = new AsyncToken<Void>();

            _config = config;
            var environment = _config.Network.Environment(_config.Network.Current);

            // shave off protocol
            var substring = environment.BaseUrl.Substring(
                environment.BaseUrl.IndexOf("://") + 3);

            var wsUrl = string.Format(
                "ws://{0}:{1}/socket.io/?EIO=2&transport=websocket&__sails_io_sdk_version=0.11.0",
                substring,
                environment.Port);
            Log.Info(this, "Connecting to {0}.", wsUrl);

            _socket = new WebSocket(wsUrl);
            {
                _socket.OnOpen += Socket_OnOpen;
                _socket.OnClose += Socket_OnClose;
                _socket.OnMessage += Socket_OnMessage;
                _socket.OnError += Socket_OnError;
                _socket.Connect();
            }
            
            return token;
        }

        private void Socket_OnOpen(object sender, EventArgs eventArgs)
        {
            Log.Info(this, "Open.");

            var req = new WebSocketRequestData
            {
                Method = "post",
                Url = string.Format(
                    "/v1/editor/app/{0}/subscribe",
                    _config.Play.AppId),
                Headers = new WebSocketRequestData.HeaderData
                {
                    Authorization = "Bearer " + _config.Network.Credentials(_config.Network.Current).Token
                }
            };

            byte[] bytes;
            _json.Serialize(req, out bytes);

            var str = "42[\"post\", " + Encoding.UTF8.GetString(bytes) + "]";
            Log.Info(this, "Sending: " + str);
            _socket.Send(str);
        }

        private void Socket_OnClose(object sender, CloseEventArgs closeEventArgs)
        {
            Log.Info(this, "Close.");
        }

        private void Socket_OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            Log.Info(this, "Message : {0}.", messageEventArgs.Data);
        }

        private void Socket_OnError(object sender, ErrorEventArgs errorEventArgs)
        {
            Log.Error(this, "Error : {0}.", errorEventArgs.Message);
        }
    }
}