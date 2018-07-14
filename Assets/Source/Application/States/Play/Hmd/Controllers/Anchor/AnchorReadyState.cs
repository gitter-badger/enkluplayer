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
        /// World anchor component.
        /// </summary>
#if NETFX_CORE
        private UnityEngine.XR.WSA.WorldAnchor _anchor;
#endif

        /// <summary>
        /// True iff anchor is located.
        /// </summary>
        private bool _isLocated;

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

#if NETFX_CORE
            _anchor = _controller.Anchor.GameObject.GetComponent<UnityEngine.XR.WSA.WorldAnchor>();

            // get a good starting value
            if (_anchor)
            {
                _isLocated = _anchor.isLocated;
            }
#endif
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
#if NETFX_CORE
            if (_anchor)
            {
                // it is now located but was not before
                if (_anchor.isLocated && !_isLocated)
                {
                    _isLocated = true;
                    _controller.Renderer.Ready();
                }
                // it was previously located but it no longer
                else if (!_anchor.isLocated && _isLocated)
                {
                    _isLocated = false;
                    _controller.Renderer.Warning();
                }
            }
#endif
        }

        /// <inheritdoc />
        public void Exit()
        {

        }
    }
}