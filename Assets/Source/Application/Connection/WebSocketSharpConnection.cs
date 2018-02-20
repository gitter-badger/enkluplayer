using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
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
        /// Message queue. This is added to from another thread.
        /// </summary>
        private readonly List<MessageEventArgs> _messages = new List<MessageEventArgs>();

        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Handles messages from connections.
        /// </summary>
        private readonly ConnectionMessageHandler _handler;

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// The underlying WebSocket.
        /// </summary>
        private WebSocket _socket;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebSocketSharpConnection(
            ApplicationConfig config,
            ConnectionMessageHandler handler,
            IBootstrapper bootstrapper)
        {
            _config = config;
            _handler = handler;
            _bootstrapper = bootstrapper;

            _bootstrapper.BootstrapCoroutine(ConsumeMessages());
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
        public void Send(WebSocketRequestRequest req)
        {
            req.Headers = new WebSocketRequestRequest.HeaderData
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
        /// Long running generator to pull messages off the queue.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ConsumeMessages()
        {
            while (true)
            {
                MessageEventArgs[] messages = null;
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
                    for (var i = 0; i < messages.Length; i++)
                    {
                        var message = messages[i];
                        _handler.OnMessage(message.Data);
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Called when socket is opened.
        /// </summary>
        private void Socket_OnOpen(object sender, EventArgs eventArgs)
        {
            Log.Info(this, "Open.");

            // immediately subscribe
            Send(new WebSocketRequestRequest(
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
            Log.Info(this, "Close.");
        }

        /// <summary>
        /// Called when socket has a message.
        /// </summary>
        private void Socket_OnMessage(object sender, MessageEventArgs messageEventArgs)
        {
            Log.Info(this, "Message : {0}.", messageEventArgs.Data);

            lock (_messages)
            {
                _messages.Add(messageEventArgs);
            }
        }

        /// <summary>
        /// Called when socket has error.
        /// </summary>
        private void Socket_OnError(object sender, ErrorEventArgs errorEventArgs)
        {
            Log.Error(this, "Error : {0}.", errorEventArgs.Message);
        }

        /// <summary>
        /// Verbose logs.
        /// </summary>
        //[Conditional("LOGGING_VERBOSE")]
        private void LogVerbose(string format, params object[] replacements)
        {
            Log.Info(this, format, replacements);
        }
    }
}