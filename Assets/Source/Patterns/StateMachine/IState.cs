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
        void Enter();

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