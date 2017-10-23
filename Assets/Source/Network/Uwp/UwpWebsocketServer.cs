#if NETFX_CORE
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
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
        private WebSocket _socket;

        /// <summary>
        /// Messages received but not yet processed.
        /// </summary>
        private readonly List<string> _messages = new List<string>();

        public int Port { get; set; }

        public UwpWebsocketServer(
            IBootstrapper bootstrapper,
            IUwpWebsocketService service)
        {
            Port = 4649;

            _service = service;

            bootstrapper.BootstrapCoroutine(ConsumeMessages());
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

        public void Send(string payload)
        {
            if (null == _socket)
            {
                return;
            }

            _socket.Send(payload);
        }

        public void Connected(WebSocket socket)
        {
            if (null != _socket)
            {
                Log.Warning(this, "Refusing connection: we are already connected.");
                socket.Close();
                return;
            }

            _socket = socket;
            _socket.ConnectionClosed += Socket_OnConnectionClosed;
            _socket.DataReceived += Socket_OnDataReceived;

            _service.OnOpen();
        }

        /// <summary>
        /// Generator that consumes messages off the queue.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ConsumeMessages()
        {
            while (true)
            {
                string[] messages = null;
                lock (_messages)
                {
                    if (_messages.Count > 0)
                    {
                        messages = _messages.ToArray();
                        _messages.Clear();
                    }
                }

                if (null != messages)
                {
                    for (int i = 0, len = messages.Length; i < len; i++)
                    {
                        _service.OnMessage(messages[i]);
                    }
                }

                yield return null;
            }
        }

        private void Socket_OnConnectionClosed(WebSocket socket)
        {
            _socket = null;
            _service.OnClose();
        }

        private void Socket_OnDataReceived(WebSocket socket, string frame)
        {
            lock (_messages)
            {
                _messages.Add(frame);
            }
        }
    }
}
#endif