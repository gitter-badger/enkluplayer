#if UNITY_EDITOR || UNITY_IOS
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEditor.Build;
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
        /// Backing variable for property.
        /// </summary>
        private bool _isConnected;

        /// <inheritdoc />
        public bool IsConnected {
            get { return _isConnected; }
            set
            {
                var prev = _isConnected;

                _isConnected = value;

                if (!prev && _isConnected && null != OnConnected)
                {
                    OnConnected();
                }
            }
        }

        /// <inheritdoc />
        public event Action OnConnected;

        /// <summary>
        /// Endpoint we are connecting to.
        /// </summary>
        private string _wsEndpoint;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public WebSocketSharpConnection(
            IMessageRouter messages,
            ApplicationConfig config,
            ConnectionMessageHandler handler)
        {
            _config = config;
            _handler = handler;
            _handler.OnHeartbeatRequested += Handler_OnSendPong;
            _handler.OnTimeout += Handler_OnTimeout;

            messages.Subscribe(
                MessageTypes.APPLICATION_SUSPEND,
                Messages_OnApplicationSuspend);
            messages.Subscribe(
                MessageTypes.APPLICATION_RESUME,
                Messages_OnApplicationResume);
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Connect(EnvironmentData environment)
        {
            _wsEndpoint = WsUrl(environment);
            
            return ConnectSocket(_wsEndpoint);
        }

        /// <summary>
        /// Sends a request.
        /// </summary>
        /// <param name="req">The request.</param>
        private void Send(WebSocketRequest req)
        {
            req.Headers = new WebSocketRequest.HeaderData
            {
                Authorization = "Bearer " + _config.Network.Credentials.Token
            };
            
            byte[] bytes;
            try
            {
                _json.Serialize(req, out bytes);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not serialize request : {0}", exception);
                return;
            }

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
        /// Creates a new socket.
        /// </summary>
        /// <param name="wsUrl">The websocket url.</param>
        /// <returns></returns>
        private IAsyncToken<Void> ConnectSocket(string wsUrl)
        {
            if (null != _connectToken)
            {
                return _connectToken.Token();
            }
            
            Log.Info(this, "Opening socket to {0}.", wsUrl);

            var token = _connectToken = new AsyncToken<Void>();

            try
            {
                _socket = new WebSocket(wsUrl);
                _socket.EmitOnPing = true;
                _socket.OnOpen += Socket_OnOpen;
                _socket.OnClose += Socket_OnClose;
                _socket.OnMessage += Socket_OnMessage;
                _socket.OnError += Socket_OnError;
                _socket.Connect();
            }
            catch (Exception exception)
            {
                token.Fail(exception);
            }

            return token.Token();
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

            IsConnected = true;

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

            IsConnected = false;
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
            LogVerbose("Error : {0} : {1}", errorEventArgs.Message, errorEventArgs.Exception);
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
        /// Called when the connection times out.
        /// </summary>
        private void Handler_OnTimeout()
        {
            Log.Warning(this, "Network timeout.");

            IsConnected = false;

            if (null != _connectToken)
            {
                _connectToken.Fail(new Exception("Timed out."));
                _connectToken = null;
            }

            ConnectSocket(_wsEndpoint);
        }

        /// <summary>
        /// Called when the application is suspended.
        /// </summary>
        private void Messages_OnApplicationSuspend(object obj)
        {
            Log.Info(this, "App suspended, killing socket.");

            IsConnected = false;

            if (null != _socket)
            {
                _socket.Close();
            }

            if (null != _connectToken)
            {
                _connectToken.Fail(new Exception("Application suspended."));
            }
        }
        
        /// <summary>
        /// Called when the application is resumed.
        /// </summary>
        private void Messages_OnApplicationResume(object obj)
        {
            if (!string.IsNullOrEmpty(_wsEndpoint))
            {
                Log.Info(this, "App resumed, reconnecting socket.");
                
                ConnectSocket(_wsEndpoint);
            }
        }
        
        /// <summary>
        /// Creates the websocket URL.
        /// </summary>
        /// <param name="environment">The environment to connect to.</param>
        /// <returns></returns>
        private static string WsUrl(EnvironmentData environment)
        {
            var url = environment.TrellisUrl.Replace("http", "ws");

            // shave off version
            var substrings = url.Split('/');
            if (substrings.Length > 3)
            {
                url = string.Join("/", substrings.Take(3).ToArray());
            }

#if UNITY_IOS
            // IOS HACK!!!
            var wsUrl = "wss://ec2-34-216-59-227.us-west-2.compute.amazonaws.com:10001/socket.io/?nosession=true&__sails_io_sdk_version=1.2.1&__sails_io_sdk_platform=browser&__sails_io_sdk_language=javascript&EIO=3&transport=websocket";
#else
            var wsUrl = string.Format(
                "{0}/socket.io/?nosession=true&__sails_io_sdk_version=1.2.1&__sails_io_sdk_platform=browser&__sails_io_sdk_language=javascript&EIO=3&transport=websocket",
                url);
#endif

            return wsUrl;
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