namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State while anchor is being moved.
    /// </summary>
    public class AnchorMovingState : IState
    {
        /// <summary>
        /// The controller.
        /// </summary>
        private readonly AnchorDesignController _controller;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorMovingState(AnchorDesignController controller)
        {
            _controller = controller;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _controller.Renderer.Editing();

        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            // do nothing
        }

        /// <inheritdoc />
        public void Exit()
        {
            
        }
    }
}