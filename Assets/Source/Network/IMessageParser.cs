namespace CreateAR.Spire
{
    /// <summary>
    /// Defines an interface for parsing messages from the web page.
    /// </summary>
    public interface IMessageParser
    {
        /// <summary>
        /// Parses a message.
        /// </summary>
        /// <param name="message">The message to be parsed.</param>
        /// <param name="messageType">The message type.</param>
        /// <param name="payloadString">Payload string.</param>
        /// <returns>True iff the message could be parsed.</returns>
        bool ParseMessage(
            string message,
            out string messageType,
            out string payloadString);
    }
}