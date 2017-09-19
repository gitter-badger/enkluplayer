namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Handles messages from an <c>IBridge</c> implementation.
    /// </summary>
    public interface IBridgeMessageHandler
    {
        /// <summary>
        /// Allows binding between a message type and a C# type.
        /// </summary>
        MessageTypeBinder Binder { get; }

        /// <summary>
        /// Call when a message is received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        void OnMessage(string message);
    }
}