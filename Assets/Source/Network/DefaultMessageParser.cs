namespace CreateAR.Spire
{
    /// <summary>
    /// Default method the WebBridge uses to parse messages from the web page.
    /// </summary>
    public class DefaultMessageParser : IMessageParser
    {
        /// <inheritdoc cref="IMessageParser"/>
        public bool ParseMessage(
            string message,
            out string messageType,
            out string payloadString)
        {
            messageType = string.Empty;
            payloadString = string.Empty;

            var substrings = message.Split(new [] {';'}, 2);
            if (2 != substrings.Length)
            {
                return false;
            }

            messageType = substrings[0];
            payloadString = substrings[1];
            return true;
        }
    }
}