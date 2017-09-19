using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Receives messages from a bridge.
    /// </summary>
    public class BridgeMessageHandler : IBridgeMessageHandler
    {
        /// <summary>
        /// Routes messages.
        /// </summary>
        private readonly IMessageRouter _router;

        /// <inheritdoc cref="IBridgeMessageHandler"/>
        public MessageTypeBinder Binder { get; private set; }

        /// <summary>
        /// Creates a new <c>BridgeMessageHandler</c>.
        /// </summary>
        /// <param name="router"></param>
        public BridgeMessageHandler(IMessageRouter router)
        {
            _router = router;

            Binder = new MessageTypeBinder();
        }

        /// <inheritdoc cref="IBridgeMessageHandler"/>
        public void OnMessage(string message)
        {
            Commons.Unity.Logging.Log.Debug(this, "Received [{0}]", message);

            // parse
            string messageTypeString;
            string payloadString;
            if (!ParseMessage(
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
                    "Received a message for which we do not have a binding : {0}.",
                    messageTypeString);
                return;
            }

            object payload;
            if (binding.Type == typeof(Void))
            {
                payload = Void.Instance;
            }
            else if (binding.Type == typeof(string))
            {
                payload = payloadString;
            }
            else
            {
                try
                {
                    // eek-- Newtonsoft is failing me on webgl
                    payload = JsonConvert.DeserializeObject(
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

        /// <summary>
        /// Parses a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">The message type.</param>
        /// <param name="payloadString">The payload.</param>
        /// <returns></returns>
        public bool ParseMessage(
            string message,
            out string messageType,
            out string payloadString)
        {
            var parsedMessage = JObject.Parse(message);
            var type = parsedMessage.SelectToken("messageType");
            if (null == type)
            {
                messageType = payloadString = null;
                return false;
            }
            messageType = type.ToString();

            var payload = parsedMessage.SelectToken("message");
            if (null == payload)
            {
                payloadString = string.Empty;
                return true;
            }

            payloadString = payload.ToString();
            return true;
        }
    }
}