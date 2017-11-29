using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Visual characteristics common to all buttons.
    /// </summary>
    public class ActivatorState : Element, IState
    {
        /// <summary>
        /// Activator.
        /// </summary>
        public Activator Activator { get; set; }

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<float> _propFrameScale;
        private ElementSchemaProp<int> _propFrameColor;
        private ElementSchemaProp<int> _propTween;

        /// <summary>
        /// Color of the button during this state.
        /// </summary>
        public VirtualColor FrameColor { get { return (VirtualColor)_propFrameColor.Value; } }

        /// <summary>
        /// Color of the button during this state.
        /// </summary>
        public float FrameScale { get { return _propFrameScale.Value; } }

        /// <summary>
        /// Tween into this state.
        /// </summary>
        public TweenType Tween { get { return (TweenType)_propTween.Value; } }
        
        /// <summary>
        /// Prop initialization.
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _propFrameColor = Schema.Get<int>("frameColor");
            _propTween = Schema.Get<int>("tween");
            _propFrameScale = Schema.Get<float>("frameScale");
        }

        /// <summary>
        /// Invoked when the state is entered.
        /// </summary>
        /// <param name="context"></param>
        public virtual void Enter(object context)
        {
            // empty.
        }

        /// <summary>
        /// Invoked once every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(float deltaTime)
        {
            // empty.
        }

        /// <summary>
        /// Invoked when the state is exited.
        /// </summary>
        public virtual void Exit()
        {
            // empty.
        }
    }
}