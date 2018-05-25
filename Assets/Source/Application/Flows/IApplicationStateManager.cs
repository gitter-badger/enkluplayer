namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that can change both application states and flows.
    /// </summary>
    public interface IApplicationStateManager
    {
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