namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Basic interface for a state.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when the state is transitioned to.
        /// </summary>
        /// <param name="context">Optionally passed in to state.</param>
        void Enter(object context);

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">Delta time.</param>
        void Update(float dt);

        /// <summary>
        /// Called when the state is transitioned out.
        /// </summary>
        void Exit();
    }
}