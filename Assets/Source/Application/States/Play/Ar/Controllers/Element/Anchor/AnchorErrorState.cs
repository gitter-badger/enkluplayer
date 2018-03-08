using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Error state for anchors.
    /// </summary>
    public class AnchorErrorState : IState
    {
        /// <summary>
        /// The controller.
        /// </summary>
        private readonly AnchorDesignController _controller;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorErrorState(AnchorDesignController controller)
        {
            _controller = controller;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _controller.Unlock();
            _controller.Color = Color.red;
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