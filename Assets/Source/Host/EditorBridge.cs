#if UNITY_EDITOR
using System;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Spire;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class EditorBridge : WebSocketBehavior, IBridge
    {
        private class Command
        {
            public string messageType;
        }

        private readonly JsonSerializer _serializer = new JsonSerializer();

        /// <summary>
        /// Routes messages.
        /// </summary>
        private readonly IMessageRouter _router;

        /// <summary>
        /// Parses messages.
        /// </summary>
        private readonly IMessageParser _parser;

        /// <summary>
        /// WebSocket server.
        /// </summary>
        private readonly WebSocketServer _server;

        /// <summary>
        /// Token for init.
        /// </summary>
        private readonly AsyncToken<Void> _initToken = new AsyncToken<Void>();

        public DataBinder Binder { get; private set; }

        public EditorBridge(
            IMessageRouter router,
            IMessageParser parser)
        {
            _router = router;
            _parser = parser;

            Binder = new DataBinder();

            // listen for connections
            _server = new WebSocketServer("ws://localhost:4649");
            _server.AddWebSocketService(
                "/bridge",
                () => this);
            _server.Start();

            SendType("init");
        }

        public void BroadcastReady()
        {
            _initToken.OnSuccess(_ => SendType("ready"));
        }
        
        private void SendType(string messageType)
        {
            byte[] bytes;
            _serializer.Serialize(
                new Command
                {
                    messageType = messageType
                },
                out bytes);

            var payload = Encoding.UTF8.GetString(bytes);

            Commons.Unity.Logging.Log.Info(this, "Sending {0}.", payload);

            SendAsync(
                payload,
                result =>
                {
                    if (!result)
                    {
                        Commons.Unity.Logging.Log.Error(this, "Could not send message.");
                    }
                });
        }
            
        protected override void OnOpen()
        {
            base.OnOpen();
                
            Commons.Unity.Logging.Log.Info(this, "Connection opened.");

            SendType("init");

            _initToken.Succeed(Void.Instance);
        }

        protected override void OnMessage(MessageEventArgs @event)
        {
            base.OnMessage(@event);

            var message = @event.Data;

            Commons.Unity.Logging.Log.Debug(this, "Received [{0}]", message);

            // parse
            string messageTypeString;
            string payloadString;
            if (!_parser.ParseMessage(
                message,
                out messageTypeString,
                out payloadString))
            {
                Commons.Unity.Logging.Log.Warning(
                    this,
                    "Received a message that cannot be parsed : {0}.", message);
                return;
            }

            var binding = Binder.ByMessageType(messageTypeString);
            if (null == binding)
            {
                Commons.Unity.Logging.Log.Fatal(
                    this,
                    "Receieved a message for which we do not have a binding : {0}.",
                    messageTypeString);
                return;
            }

            object payload;
            if (binding.Type == typeof(Void))
            {
                payload = Void.Instance;
            }
            else
            {
                try
                {
                    // eek-- Newtonsoft is failing me on webgl
                    payload = JsonUtility.FromJson(
                        payloadString,
                        binding.Type);
                }
                catch (Exception exception)
                {
                    Commons.Unity.Logging.Log.Error(
                        this,
                        "Could not deserialize {0} payload to a [{1}] : {2}.",
                        messageTypeString,
                        binding.Type,
                        exception);
                    return;
                }
            }

            Commons.Unity.Logging.Log.Debug(this,
                "Publishing a {0} event.",
                messageTypeString);

            // publish
            _router.Publish(
                binding.MessageTypeInt,
                payload);
        }

        protected override void OnClose(CloseEventArgs @event)
        {
            base.OnClose(@event);

            Commons.Unity.Logging.Log.Info(this, "Closed.");
        }
    }
}
#endif