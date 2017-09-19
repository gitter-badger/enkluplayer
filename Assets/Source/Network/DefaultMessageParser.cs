using CreateAR.Commons.Unity.Async;
using Newtonsoft.Json.Linq;

namespace CreateAR.Spire
{
    /// <summary>
    /// Default method for parsing messages from the web page.
    /// </summary>
    public class DefaultMessageParser : IMessageParser
    {
        /// <inheritdoc cref="IMessageParser"/>
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