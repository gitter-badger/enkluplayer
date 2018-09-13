namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for input.
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// <c>MultiInput</c> implementation.
        /// </summary>
        IMultiInput MultiInput { get; }

        /// <summary>
        /// Changes the state of input.
        /// </summary>
        /// <param name="state">The state to transition to.</param>
        void ChangeState(IState state);

        /// <summary>
        /// Updates the input state.
        /// </summary>
        /// <param name="dt">Time since last update.</param>
        void Update(float dt);
    }
}