namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can change both application states and flows.
    /// </summary>
    public interface IApplicationStateManager
    {
        /// <summary>
        /// Listens for messages that flows will use and passes them along to flows.
        /// </summary>
        /// <param name="messageTypes">Message types to listen to.</param>
        void ListenForFlowMessages(params int[] messageTypes);
        
        /// <summary>
        /// Changes states.
        /// </summary>
        /// <param name="context">Any object we wish to pass to the next state.</param>
        void ChangeState<T>(object context = null) where T : IState;
        
        /// <summary>
        /// Changes flows.
        /// </summary>
        void ChangeFlow<T>() where T : IStateFlow;
    }
}