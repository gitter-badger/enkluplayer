#if UNITY_EDITOR || UNITY_IOS
using System;
using System.Diagnostics;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using WebSocketSharp;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// WebSocket based <c>IConnection</c>.
    /// </summary>
    public class WebSocketSharpConnection : IConnection
    {
        /// <summary>
        /// Serializer.
        /// </summary>
        private readonly JsonSerializer _json = new JsonSerializer();
        
        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Handles messages from connections.
        /// </summary>
        private readonly ConnectionMessageHandler _handler;
        
        /// <summary>
        /// The underlying WebSocket.
        /// </summary>
        private WebSocket _socket;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebSocketSharpConnection(
            ApplicationConfig config,
            ConnectionMessageHandler handler)
        {
            _config = config;
            _handler = handler;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Connect(EnvironmentData environment)
        {
            var token = new AsyncToken<Void>();

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

        /// <summary>
        /// Sends a request.
        /// </summary>
        /// <param name="req">The request.</param>
        public void Send(WebSocketRequest req)
        {
            req.Headers = new WebSocketRequest.HeaderData
            {
                Authorization = "Bearer " + _config.Network.Credentials(_config.Network.Current).Token
            };

            byte[] bytes;
            _json.Serialize(req, out bytes);

            var str = "42[\"post\", " + Encoding.UTF8.GetString(bytes) + "]";

            if (null != req.Data)
            {
                LogVerbose("{0} {1}: {2}",
                    req.Method,
                    req.Url,
                    req.Data);
            }
            else
            {
                LogVerbose("{0} {1}",
                    req.Method,
                    req.Url);
            }

            _socket.Send(str);
        }
        
        /// <summary>
        /// Called when socket is opened.
        /// </summary>
        private void Socket_OnOpen(object sender, EventArgs eventArgs)
        {
            Log.Info(this, "Socket connected.");

            // immediately subscribe
            Send(new WebSocketRequest(
                string.Format(
                    "/v1/editor/app/{0}/subscribe",
                    _config.Play.AppId),
                "post"));
        }
        
        /// <summary>
        /// Called when socket is closed.
        /// </summary>
        private void Socket_OnClose(object sender, CloseEventArgs closeEventArgs)
        {
            LogVerbose("Socket closed.");
        }

        /// <summary>
        /// Called when socket has a message.
        /// </summary>
        private void Socket_OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            LogVerbose("Message : {0}.", messageEventArgs.Data);

            _handler.OnMessage(messageEventArgs.Data);
        }

        /// <summary>
        /// Called when socket has error.
        /// </summary>
        private void Socket_OnError(object sender, ErrorEventArgs errorEventArgs)
        {
            LogVerbose("Error : {0}.", errorEventArgs.Message);
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