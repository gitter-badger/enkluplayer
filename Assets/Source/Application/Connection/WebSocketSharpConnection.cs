﻿#if UNITY_EDITOR || UNITY_IOS
using System;
using System.Diagnostics;
using System.Linq;
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
        /// Token for connection.
        /// </summary>
        private AsyncToken<Void> _connectToken;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public WebSocketSharpConnection(
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

            _connectToken = new AsyncToken<Void>();

            // replace protocol (works for https too)
            var url = environment.Url.Replace("http", "ws");

            // shave off version
            var substrings = url.Split('/');
            if (substrings.Length > 3)
            {
                url = string.Join("/", substrings.Take(3).ToArray());
            }

            // make websocket url
            var wsUrl = string.Format(
                "{0}/socket.io/?EIO=2&transport=websocket&__sails_io_sdk_version=0.11.0",
                url);
            
            Log.Info(this, "Connecting to {0}.", wsUrl);
            
            _socket = new WebSocket(wsUrl);
            {
                _socket.EmitOnPing = true;
                _socket.OnOpen += Socket_OnOpen;
                _socket.OnClose += Socket_OnClose;
                _socket.OnMessage += Socket_OnMessage;
                _socket.OnError += Socket_OnError;
                _socket.Connect();
            }
            
            return _connectToken.Token();
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
            
            _connectToken.Succeed(Void.Instance);
        }
        
        /// <summary>
        /// Called when socket is closed.
        /// </summary>
        private void Socket_OnClose(object sender, CloseEventArgs closeEventArgs)
        {
            LogVerbose("Socket closed.");

            _connectToken.Fail(new Exception("Socket closed."));
            _connectToken = null;
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