#if NETFX_CORE

using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using IotWeb.Common.Http;
using IotWeb.Server;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Allows commands to be sent from conductor to UWP app.
    /// </summary>
    public class UwpCommandService : CommandService, IWebSocketRequestHandler
    {
        /// <summary>
        /// Uwp implementation of the <c>ICommandClient</c> interface.
        /// </summary>
        private class UwpCommandClient : ICommandClient
        {
            /// <summary>
            /// The web socket.
            /// </summary>
            private readonly WebSocket _socket;

            /// <inheritdoc />
            public event Action<ICommandClient> OnClosed;

            /// <summary>
            /// Constructor.
            /// </summary>
            public UwpCommandClient(WebSocket socket)
            {
                _socket = socket;
                _socket.ConnectionClosed += Socket_OnConnectionClosed;
            }

            /// <inheritdoc />
            public void Send(string message)
            {
                _socket.Send(message);
            }

            /// <summary>
            /// Called when the socket is closed.
            /// </summary>
            /// <param name="socket">The socket.</param>
            private void Socket_OnConnectionClosed(WebSocket socket)
            {
                OnClosed?.Invoke(this);
            }
        }

        /// <summary>
        /// Tracks messages and clients.
        /// </summary>
        private class MessageRecord
        {
            /// <summary>
            /// The message.
            /// </summary>
            public string Message;

            /// <summary>
            /// The client associated with the message.
            /// </summary>
            public ICommandClient Client;
        }

        /// <summary>
        /// Port on which to run command server.
        /// </summary>
        private const int PORT = 4224;

        /// <summary>
        /// Bootstrapper.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Underlying server.
        /// </summary>
        private readonly HttpServer _server;
        
        /// <summary>
        /// Messages received but not yet processed.
        /// </summary>
        private readonly List<MessageRecord> _queue = new List<MessageRecord>();

        /// <summary>
        /// List of connected clients.
        /// </summary>
        private readonly Dictionary<WebSocket, ICommandClient> _clients = new Dictionary<WebSocket, ICommandClient>();

        /// <summary>
        /// Functions that can handle messages.
        /// </summary>
        private readonly Dictionary<string, Action<string, ICommandClient>> _handlers = new Dictionary<string, Action<string, ICommandClient>>();

        /// <summary>
        /// True iff coroutine is alive.
        /// </summary>
        private int _isAliveId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UwpCommandService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            IBootstrapper bootstrapper)
            : base(binder, messages)
        {
            _bootstrapper = bootstrapper;

            _server = new HttpServer(PORT);
            _server.AddWebSocketRequestHandler("/command", this);
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            _bootstrapper.BootstrapCoroutine(ProcessMessages());

            _server.Start();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            _server.Stop();

            // discard remaining messages
            lock (_queue)
            {
                _queue.Clear();
            }

            _isAliveId = -1;
        }

        /// <inheritdoc />
        public bool WillAcceptRequest(string uri, string protocol)
        {
            return true;
        }

        /// <inheritdoc />
        public void Connected(WebSocket socket)
        {
            _clients[socket] = new UwpCommandClient(socket);

            socket.ConnectionClosed += Socket_OnConnectionClosed;
            socket.DataReceived += Socket_OnDataReceived;
        }

        /// <inheritdoc />
        public override void SetHandler(string type, Action<string, ICommandClient> handler)
        {
            base.SetHandler(type, handler);

            _handlers[type] = handler;
        }

        /// <inheritdoc />
        public override void RemoveHandler(string type)
        {
            base.RemoveHandler(type);

            _handlers.Remove(type);
        }

        /// <summary>
        /// Messages come in on a different thread, so this method processes
        /// them on the main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ProcessMessages()
        {
            var prng = new Random();
            var id = _isAliveId = prng.Next();

            while (id == _isAliveId)
            {
                MessageRecord[] records = null;
                lock (_queue)
                {
                    if (_queue.Count > 0)
                    {
                        records = _queue.ToArray();
                        _queue.Clear();
                    }
                }

                if (null != records)
                {
                    for (int i = 0, len = records.Length; i < len; i++)
                    {
                        var record = records[i];
                        var message = record.Message;

                        // parse
                        var substrings = message.Split(new [] { ' ' }, 2);

                        // handle
                        if (!_handlers.TryGetValue(substrings[0], out var handler))
                        {
                            Log.Error(this, "Could not find handler for command message [{0}].", message);
                        }
                        else
                        {
                            Log.Info(this, "Handling command: [{0}].", message);

                            handler(message, record.Client);
                        }
                    }
                }

                yield return null;
            }
        }
        
        /// <summary>
        /// Called when a socket connection is closed.
        /// </summary>
        /// <param name="socket">The socket.</param>
        private void Socket_OnConnectionClosed(WebSocket socket)
        {
            _clients.Remove(socket);
        }

        /// <summary>
        /// Called when a socket receives a message.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="frame">The WebSocket frame.</param>
        private void Socket_OnDataReceived(WebSocket socket, string frame)
        {
            lock (_messages)
            {
                _queue.Add(new MessageRecord
                {
                    Message = frame,
                    Client = _clients[socket]
                });
            }
        }
    }
}

#endif