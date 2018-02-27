#if NETFX_CORE
using System;
using System.Diagnostics;
using System.Text;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IConnection</c> implementation for UWP.
    /// </summary>
    public class UwpConnection : IConnection
    {
        /// <summary>
        /// Application config.
        /// </summary>
        private readonly ApplicationConfig _config;
        
        /// <summary>
        /// Handles messages from connections.
        /// </summary>
        private readonly ConnectionMessageHandler _handler;

        /// <summary>
        /// Serializes JSON.
        /// </summary>
        private readonly JsonSerializer _json = new JsonSerializer();

        /// <summary>
        /// Environment we are connected to.
        /// </summary>
        private EnvironmentData _environment;

        /// <summary>
        /// Token used during connection. If null, we are not connecting.
        /// </summary>
        private AsyncToken<Void> _connectToken;

        /// <summary>
        /// Underlying socket. If null, we are not connected.
        /// </summary>
        private MessageWebSocket _socket;

        /// <summary>
        /// Writes data to socket.
        /// </summary>
        private DataWriter _writer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UwpConnection(
            ApplicationConfig config,
            ConnectionMessageHandler handler)
        {
            _config = config;
            _handler = handler;
            _handler.OnHeartbeatRequested += Handler_OnSendPong;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Connect(EnvironmentData environment)
        {
            if (null != _connectToken)
            {
                return _connectToken.Token();
            }

            _environment = environment;
            _connectToken = new AsyncToken<Void>();

            // clear token after resolve
            _connectToken.OnFinally(_ => _connectToken = null);

            // strip protocol
            var substring = _environment.BaseUrl.Substring(
                _environment.BaseUrl.IndexOf("://", StringComparison.Ordinal) + 3);

            var wsUrl = string.Format(
                "ws://{0}:{1}/socket.io/?EIO=2&transport=websocket&__sails_io_sdk_version=0.11.0",
                substring,
                environment.Port);

            ConnectAsync(_connectToken, wsUrl);

            return _connectToken.Token();
        }

        /// <summary>
        /// Sends a request.
        /// </summary>
        /// <param name="req">the request.</param>
        public void Send(WebSocketRequest req)
        {
            if (null == req.Headers)
            {
                req.Headers = new WebSocketRequest.HeaderData();
            }

            req.Headers.Authorization = "Bearer " + _config.Network.Credentials(
                                            _config.Network.Current).Token;

            _json.Serialize(req, out var bytes);

            var message = "42[\"post\", " + Encoding.UTF8.GetString(bytes) + "]";

            SendAsync(message);
        }

        /// <summary>
        /// Connects asynchronously.
        /// </summary>
        /// <param name="token">The token to resolve.</param>
        /// <param name="wsUrl">The websocket URL.</param>
        private async void ConnectAsync(AsyncToken<Void> token, string wsUrl)
        {
            LogVerbose("ConnectAsync({0})", wsUrl);

            _socket = new MessageWebSocket();
            _socket.Control.MessageType = SocketMessageType.Utf8;
            _socket.SetRequestHeader(
                "Authorization",
                "Bearer " + _config.Network.Credentials(_config.Network.Current).Token);

            _socket.Closed += Socket_OnClosed;
            _socket.MessageReceived += Socket_OnMessageReceived;
            _socket.ServerCustomValidationRequested += Socket_OnServerCustomValidationRequested;

            try
            {
                await _socket.ConnectAsync(new Uri(wsUrl));

                LogVerbose("Connected to {0}", wsUrl);

                _writer = new DataWriter(_socket.OutputStream);

                // subscribe
                Send(new WebSocketRequest(
                    string.Format(
                        "/v1/editor/app/{0}/subscribe",
                        _config.Play.AppId),
                    "post"));

                token.Succeed(Void.Instance);
            }
            catch (Exception exception)
            {
                token.Fail(exception);
            }
        }

        /// <summary>
        /// Sends a message asynchronously.
        /// </summary>
        /// <param name="message">The message to send.</param>
        private async void SendAsync(string message)
        {
            LogVerbose("Sending : {0}.", message);

            _writer.WriteString(message);

            try
            {
                await _writer.StoreAsync();
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not send message : {0}.", exception);
            }
        }

        /// <summary>
        /// Called when the socket has been closed.
        /// </summary>
        private void Socket_OnClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            Log.Info(this, "Socket closed. Attempting reconnect.");

            Connect(_environment)
                .OnSuccess(_ => LogVerbose("Reconnect successful."))
                .OnFailure(_ => LogVerbose("Reconnect failed!"));
        }

        /// <summary>
        /// Called when a message has been received.
        /// </summary>
        private void Socket_OnMessageReceived(
            MessageWebSocket sender,
            MessageWebSocketMessageReceivedEventArgs args)
        {
            using (var reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                try
                {
                    var message = reader.ReadString(reader.UnconsumedBufferLength);

                    LogVerbose("Message : {0}.", message);

                    _handler.OnMessage(message);
                }
                catch (Exception exception)
                {
                    Log.Error(this,
                        "Error reading message : {0}.",
                        exception);
                }
            }
        }

        /// <summary>
        /// Called when the socket server requests custom validation.
        /// </summary>
        private void Socket_OnServerCustomValidationRequested(
            MessageWebSocket sender,
            WebSocketServerCustomValidationRequestedEventArgs args)
        {
            Log.Info(this, "Socket requesting validation.");
        }

        /// <summary>
        /// Called when the handler tells the connection to send a pong.
        /// </summary>
        private void Handler_OnSendPong()
        {
            LogVerbose("Pong()");

            _socket.Send("40");
        }

        /// <summary>
        /// Verbose logs.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void LogVerbose(string format, params object[] replacements)
        {
            Log.Info(this, format, replacements);
        }
    }
}
#endif