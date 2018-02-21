namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can filter messages.
    /// </summary>
    public interface IMessageExclusionFilter
    {
        /// <summary>
        /// True iff the filter wants to exclude the message.
        /// </summary>
        /// <param name="messageType">The message type.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        bool Exclude(int messageType, object message);
    }
}