using System;
using System.Diagnostics;
using CreateAR.Commons.Unity.Logging;
using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Receives messages from a connection.
    /// </summary>
    public class ConnectionMessageHandler
    {
        /// <summary>
        /// Filters messages for router.
        /// </summary>
        private readonly MessageFilter _filter;

        /// <summary>
        /// Binds message types to C# types.
        /// </summary>
        private readonly MessageTypeBinder _binder;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ConnectionMessageHandler(
            MessageFilter filter,
            MessageTypeBinder binder)
        {
            _filter = filter;
            _binder = binder;
        }

        /// <summary>
        /// Handles messages from connection.
        /// </summary>
        /// <param name="message">The message.</param>
        public void OnMessage(string message)
        {
            var header = "42[\"message\",";
            if (message.StartsWith(header))
            {
                message = message.Substring(header.Length);
                message = message.TrimEnd(']');
            }
            else
            {
                return;
            }
            
            LogVerbose("Message Received: {0}", message);

            JsonValue parsed;
            int messageType;

            try
            {
                parsed = JsonValue.Parse(message);
                messageType = parsed["type"].AsInteger;
            }
            catch (InvalidOperationException)
            {
                // invalid json
                return;
            }
            
            LogVerbose("\tMessage type: {0}", messageType);

            var type = _binder.ByMessageType(messageType);

            LogVerbose("\tBound type: {0}", type);

            if (null == type)
            {
                Log.Warning(this,
                    "Binder had no type for [{0}] : {1}.",
                    messageType,
                    message);

                return;
            }

            var typedMessage = parsed.As(type);

            LogVerbose("\tMessage: {0}", typedMessage);

            _filter.Publish(messageType, typedMessage);
        }

        /// <summary>
        /// Logs.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}