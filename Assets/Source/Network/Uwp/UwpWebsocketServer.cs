#if NETFX_CORE
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using IotWeb.Common.Http;
using IotWeb.Server;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Sets up a websocket server to listen for clients.
    /// </summary>
    public class UwpWebsocketServer : IWebSocketRequestHandler
    {
        /// <summary>
        /// service to receive messages.
        /// </summary>
        private readonly IUwpWebsocketService _service;

        /// <summary>
        /// Underlying server.
        /// </summary>
        private HttpServer _server;

        /// <summary>
        /// Connection with client.
        /// </summary>
        private WebSocket _socket;

        /// <summary>
        /// Messages received but not yet processed.
        /// </summary>
        private readonly List<string> _messages = new List<string>();

        /// <summary>
        /// Port to start service on.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bootstrapper">Bootstraps coroutines\.</param>
        /// <param name="service">Object to listen to events.</param>
        public UwpWebsocketServer(
            IBootstrapper bootstrapper,
            IUwpWebsocketService service)
        {
            Port = 4649;

            _service = service;

            bootstrapper.BootstrapCoroutine(ConsumeMessages());
        }

        /// <summary>
        /// Starts listening.
        /// </summary>
        public void Listen()
        {
            _server = new HttpServer(Port);
            _server.AddWebSocketRequestHandler(
                "/bridge",
                this);
            _server.Start();
        }

        /// <inheritdoc cref="IWebSocketRequestHandler"/>
        public bool WillAcceptRequest(string uri, string protocol)
        {
            return true;
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="payload">The payload to send.</param>
        public void Send(string payload)
        {
            if (null == _socket)
            {
                return;
            }

            _socket.Send(payload);
        }

        /// <inheritdoc cref="IWebSocketRequestHandler"/>
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

        /// <summary>
        /// Called when the socket disconnects.
        /// </summary>
        /// <param name="socket">The socket.</param>
        private void Socket_OnConnectionClosed(WebSocket socket)
        {
            _socket = null;
            _service.OnClose();
        }

        /// <summary>
        /// Called when the socket receives data.
        /// 
        /// Note: This is called in a different thread.
        /// </summary>
        /// <param name="socket">The socket the data was received on.</param>
        /// <param name="frame">The data received.</param>
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