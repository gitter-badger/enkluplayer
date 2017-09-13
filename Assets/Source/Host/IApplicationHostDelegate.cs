namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an interface for receiving messages from the <c>IApplicationHost</c>.
    /// </summary>
    public interface IApplicationHostDelegate
    {
        /// <summary>
        /// Receives a message from the <c>IApplicationHost</c>.
        /// </summary>
        /// <param name="messageType">The type of message.</param>
        /// <param name="message">The message.</param>
        void On(int messageType, object message);
    }
}