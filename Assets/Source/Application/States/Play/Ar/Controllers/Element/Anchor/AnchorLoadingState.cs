using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State while anchor is loading.
    /// </summary>
    public class AnchorLoadingState : IState
    {
        /// <summary>
        /// The controller.
        /// </summary>
        private readonly AnchorDesignController _controller;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorLoadingState(AnchorDesignController controller)
        {
            _controller = controller;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _controller.Lock();
            _controller.Color = Color.grey;
            _controller.CloseSplash();

            return;
            ((WorldAnchorWidget) _controller.Element)
                .Import()
                .OnSuccess(_ => _controller.ChangeState<AnchorReadyState>())
                .OnFailure(_ => _controller.ChangeState<AnchorErrorState>());
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