#if NETFX_CORE
using CreateAR.Commons.Unity.Logging;
using IotWeb.Common.Http;
using IotWeb.Server;

namespace CreateAR.SpirePlayer
{
    public interface IUwpWebsocketService
    {
        void OnOpen();
        void OnMessage(string message);
        void OnClose();
    }

    public class UwpWebsocketServer : IWebSocketRequestHandler
    {
        private readonly IUwpWebsocketService _service;
        private HttpServer _server;

        public int Port { get; set; }

        public UwpWebsocketServer(IUwpWebsocketService service)
        {
            Port = 4649;

            _service = service;
        }

        public void Listen()
        {
            _server = new HttpServer(Port);
            _server.AddWebSocketRequestHandler(
                "/bridge",
                this);
            _server.Start();
        }

        public bool WillAcceptRequest(string uri, string protocol)
        {
            return true;
        }

        public void Connected(WebSocket socket)
        {
            socket.ConnectionClosed += Socket_OnConnectionClosed;
            socket.DataReceived += Socket_OnDataReceived;

            _service.OnOpen();
        }

        private void Socket_OnConnectionClosed(WebSocket socket)
        {
            _service.OnClose();
        }

        private void Socket_OnDataReceived(WebSocket socket, string frame)
        {
            _service.OnMessage(frame);
        }
    }
}
#endif