using System;
using System.Runtime.Remoting;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using WebSocketSharp;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public interface IConnection
    {
        IAsyncToken<Void> Connect(NetworkConfig config);
    }

    public class WebSocketSharpConnection : IConnection
    {
        private WebSocket _socket;

        public IAsyncToken<Void> Connect(NetworkConfig config)
        {
            var token = new AsyncToken<Void>();

            var environment = config.Environment(config.Current);

            // shave off protocol
            var substring = environment.BaseUrl.Substring(
                environment.BaseUrl.IndexOf("://") + 3);

            var wsUrl = string.Format(
                "ws://{0}:{1}/socket.io/?EIO=2&transport=websocket",
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