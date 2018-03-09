namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State while anchor is being edited.
    /// </summary>
    public class AnchorEditingState : IState
    {
        /// <summary>
        /// The controller.
        /// </summary>
        private readonly AnchorDesignController _controller;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorEditingState(AnchorDesignController controller)
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