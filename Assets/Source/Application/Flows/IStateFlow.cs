namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that determines the flow between states.
    /// </summary>
    public interface IStateFlow
    {
        /// <summary>
        /// Starts the flow. Between start and stop, MessageReceived will be
        /// called with application messages.
        /// </summary>
        /// <param name="states">An object that allows state + flow changes.</param>
        void Start(IApplicationStateManager states);
        
        /// <summary>
        /// Stops the flow.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Called when a message is recieved.
        /// </summary>
        /// <param name="messageType">The message type.</param>
        /// <param name="message">The message.</param>
        void MessageReceived(int messageType, object message);
    }
}