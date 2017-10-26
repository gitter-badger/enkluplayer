using System;
using System.Diagnostics;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using Newtonsoft.Json;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Receives messages from a bridge.
    /// </summary>
    public class BridgeMessageHandler
    {
        /// <summary>
        /// Routes messages.
        /// </summary>
        private readonly IMessageRouter _router;
        
        /// <summary>
        /// Binds message types.
        /// </summary>
        public MessageTypeBinder Binder { get; private set; }
        
        /// <summary>
        /// Creates a new <c>BridgeMessageHandler</c>.
        /// </summary>
        public BridgeMessageHandler(IMessageRouter router)
        {
            _router = router;

            Binder = new MessageTypeBinder();
        }
        
        /// <summary>
        /// Called when a message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        public void OnMessage(string message)
        {
            // no body means void message
            if (!message.Contains(":"))
            {
                DebugLog("Received : {0}", message);

                HandleVoidMessage(message);
                return;
            }
    
            // strip off payload
            var substrings = message.Split(':');
            if (2 != substrings.Length)
            {
                Log.Error(this, "Received a malformed message : {0}.",
                    message);
                return;
            }
            
            var messageType = MessageType(substrings[0]);
            var payloadType = Binder.ByMessageType(messageType);
            if (null == payloadType)
            {
                Log.Error(this,
                    "Received a message of type {0}, but there was no binding.",
                    substrings[0]);
                return;
            }
            
            // un-base64 payload
            string payloadString;
            try
            {
                payloadString = Encoding.UTF8.GetString(Convert.FromBase64String(substrings[1]));
            }
            catch (Exception exception)
            {
                Log.Info(this,
                    "Could not convert payload from base64 string [{0}]:[{1}] : {2}.",
                    messageType,
                    substrings[1],
                    exception);
                return;
            }

            DebugLog("Received : {0} : {1}", message, payloadString);

            // handle strings
            if (typeof(string) == payloadType)
            {
                _router.Publish(messageType, payloadString);
                return;
            }

            // deserialize
            try
            {
                var payload = JsonConvert.DeserializeObject(
                    payloadString,
                    payloadType);

                _router.Publish(messageType, payload);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not deserialize message [{0}]:[{1}] into type {2} : {3}.",
                    messageType,
                    payloadString,
                    payloadType,
                    exception);
            }
        }

        /// <summary>
        /// Special handling for messages with no payload.
        /// </summary>
        /// <param name="message">The complete message.</param>
        private void HandleVoidMessage(string message)
        {
            var messageType = MessageType(message);
            if (-1 == messageType)
            {
                Log.Error(this, "Received a malformed message : {0}.",
                    message);
                return;
            }

            // check binding
            var payloadType = Binder.ByMessageType(messageType);
            if (null == payloadType)
            {
                Log.Error(this,
                    "Received a message of type {0}, but there was no binding.",
                    typeof(Void));
                return;
            }

            if (payloadType != typeof(Void))
            {
                Log.Error(this,
                    "Received a message with Void message type, but expected type {0}.",
                    payloadType);
                return;
            }

            Log.Debug(this, "Received [{0}]", messageType);

            _router.Publish(messageType, Void.Instance);
        }

        /// <summary>
        /// Parses the message type from a string.
        /// </summary>
        /// <param name="stringValue">String value.</param>
        /// <returns></returns>
        private int MessageType(string stringValue)
        {
            int value;
            if (!int.TryParse(stringValue, out value))
            {
                return -1;
            }

            return value;
        }

        /// <summary>
        /// Debug logging.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="replacements">Replacements.</param>
        [Conditional("DEBUG_LOGGING")]
        private void DebugLog(object message, params object[] replacements)
        {
            Log.Debug(this, message, replacements);
        }
    }
}