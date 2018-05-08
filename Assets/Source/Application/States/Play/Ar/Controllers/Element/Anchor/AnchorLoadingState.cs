using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

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
            _controller.Renderer.Loading();
            
            var widget = (WorldAnchorWidget) _controller.Element;
            if (widget.IsAnchorLoading)
            {
                widget.OnAnchorLoadError += Widget_OnAnchorLoadError;
                widget.OnAnchorLoadSuccess += Widget_OnAnchorLoadSuccess;
            }
            else if (widget.IsAnchorLoaded)
            {
                _controller.ChangeState<AnchorReadyState>();
            }
            else
            {
                Log.Warning(this, "Bad state: anchor is neither loaded nor loading.");

                _controller.ChangeState<AnchorReadyState>();
            }
        }

        /// <inheritdoc />
        public void Update(float dt)
        {

        }

        /// <inheritdoc />
        public void Exit()
        {
            var widget = (WorldAnchorWidget) _controller.Element;

            widget.OnAnchorLoadError += Widget_OnAnchorLoadError;
            widget.OnAnchorLoadSuccess += Widget_OnAnchorLoadSuccess;
        }

        /// <summary>
        /// Called when the widget loads its anchor successfully.
        /// </summary>
        private void Widget_OnAnchorLoadSuccess()
        {
            _controller.ChangeState<AnchorReadyState>();
        }

        /// <summary>
        /// Called when the widget could not load its anchor.
        /// </summary>
        private void Widget_OnAnchorLoadError()
        {
            _controller.ChangeState<AnchorErrorState>();
        }
    }
}