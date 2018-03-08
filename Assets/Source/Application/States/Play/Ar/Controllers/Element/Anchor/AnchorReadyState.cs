namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Default state of anchor.
    /// </summary>
    public class AnchorReadyState : IState
    {
        /// <summary>
        /// The controller.
        /// </summary>
        private readonly AnchorDesignController _controller;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorReadyState(AnchorDesignController controller)
        {
            _controller = controller;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _controller.Unlock();
            _controller.Renderer.Ready();
        }

        /// <inheritdoc />
        public void Update(float dt)
        {

        }

        /// <inheritdoc />
        public void Exit()
        {

        }
    }
}