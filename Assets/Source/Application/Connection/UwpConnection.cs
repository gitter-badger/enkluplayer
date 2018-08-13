#if NETFX_CORE
using System;
using System.Diagnostics;
using System.Linq;
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

        /// <inheritdoc />
        public bool IsConnected { get; private set; }

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
            var internalToken = _connectToken = new AsyncToken<Void>();

            // clear token after resolve
            _connectToken.OnFinally(_ => _connectToken = null);

            // replace protocol (works for https too)
            var url = environment.TrellisUrl.Replace("http", "ws");

            // shave off version
            var substrings = url.Split('/');
            if (substrings.Length > 3)
            {
                url = string.Join("/", substrings.Take(3).ToArray());
            }

            // make websocket url
            var wsUrl = string.Format(
                "{0}/socket.io/?nosession=true&__sails_io_sdk_version=1.2.1&__sails_io_sdk_platform=browser&__sails_io_sdk_language=javascript&EIO=3&transport=websocket",
                url);

            Log.Info(this, "Connecting to {0}.", wsUrl);

            ConnectAsync(_connectToken, wsUrl);

            return internalToken.Token();
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

            req.Headers.Authorization = "Bearer " + _config.Network.Credentials.Token;

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
                "Bearer " + _config.Network.Credentials.Token);

            _socket.Closed += Socket_OnClosed;
            _socket.MessageReceived += Socket_OnMessageReceived;
            _socket.ServerCustomValidationRequested += Socket_OnServerCustomValidationRequested;

            try
            {
                await _socket.ConnectAsync(new Uri(wsUrl));

                LogVerbose("Connected to {0}", wsUrl);

                IsConnected = true;

                _writer = new DataWriter(_socket.OutputStream);

                // subscribe
                Send(new WebSocketRequest(
                    string.Format(
                        "/v1/editor/app/{0}/subscribe",
                        _config.Play.AppId),
                    "post"));

                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    token.Succeed(Void.Instance);
                }, false);
            }
            catch (Exception exception)
            {
                LogVerbose("Could not connect : {0}.", exception);

                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    token.Fail(exception);
                }, false);
            }
        }

        /// <summary>
        /// Sends a message asynchronously.
        /// </summary>
        /// <param name="message">The message to send.</param>
        private async void SendAsync(string message)
        {
            LogVerbose("Sending : {0}.", message);
            
            try
            {
                _writer.WriteString(message);

                await _writer.StoreAsync();

                Log.Info(this, "Message sent.");
            }
            catch (Exception exception)
            {
                LogVerbose("Could not send message : {0}.", exception);
            }
        }

        /// <summary>
        /// Called when the socket has been closed.
        /// </summary>
        private void Socket_OnClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            LogVerbose("Socket closed. Attempting reconnect.");

            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                Connect(_environment)
                    .OnSuccess(_ => LogVerbose("Reconnect successful."))
                    .OnFailure(_ => LogVerbose("Reconnect failed!"));
            }, false);
        }

        /// <summary>
        /// Called when a message has been received.
        /// </summary>
        private void Socket_OnMessageReceived(
            MessageWebSocket sender,
            MessageWebSocketMessageReceivedEventArgs args)
        {
            DataReader reader = null;
            try
            {
                reader = args.GetDataReader();
                reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                var message = reader.ReadString(reader.UnconsumedBufferLength);

                LogVerbose("Received Message : {0}.", message);

                UnityEngine.WSA.Application.InvokeOnAppThread(
                    () => _handler.OnMessage(message),
                    false);
            }
            catch (Exception exception)
            {
                Log.Warning(
                    this,
                    "Socket OnMessageReceived Exception: {0}.",
                    exception);
            }
            finally
            {
                reader?.Dispose();
            }
        }

        /// <summary>
        /// Called when the socket server requests custom validation.
        /// </summary>
        private void Socket_OnServerCustomValidationRequested(
            MessageWebSocket sender,
            WebSocketServerCustomValidationRequestedEventArgs args)
        {
            LogVerbose("Socket requesting validation.");
        }

        /// <summary>
        /// Called when the handler tells the connection to send a pong.
        /// </summary>
        private void Handler_OnSendPong()
        {
            LogVerbose("Pong()");

            SendAsync("40");
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