using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Visual characteristics common to all buttons.
    /// </summary>
    public class ActivatorState : Element, IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        public IColorConfig Colors { get; private set; }
        public ITweenConfig Tweens { get; private set; }
        public IActivator Activator { get; set; }

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<float> _propFrameScale;
        private ElementSchemaProp<int> _propFrameColor;
        private ElementSchemaProp<int> _propTween;

        /// <summary>
        /// Color of the button during this state.
        /// </summary>
        public VirtualColor FrameColor { get { return (VirtualColor)_propFrameScale.Value; } }

        /// <summary>
        /// Color of the button during this state.
        /// </summary>
        public float FrameScale { get { return _propFrameColor.Value; } }

        /// <summary>
        /// Tween into this state.
        /// </summary>
        public TweenType Tween { get { return (TweenType)_propTween.Value; } }

        /// <summary>
        /// Dependency initialization.
        /// </summary>
        public void Initialize(IColorConfig colors, ITweenConfig tweens)
        {
            Colors = colors;
            Tweens = tweens;
        }

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
            
        }

        /// <summary>
        /// Invoked once every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(float deltaTime)
        {
            var tweenDuration = Tweens.DurationSeconds(Tween);
            var tweenLerp
                = tweenDuration > Mathf.Epsilon
                    ? deltaTime / tweenDuration
                    : 1.0f;

            // blend the frame's color.
            var frame = Activator.Frame;
            var frameColor = Colors.GetColor(FrameColor);
            frame.LocalColor 
                = Col4.Lerp(
                    frame.LocalColor,
                    frameColor,
                    tweenLerp);

            // blend the frame's scale.
            frame.GameObject.transform.localScale
                = Vector3.Lerp(
                    frame.GameObject.transform.localScale,
                    Vector3.one * FrameScale,
                    tweenLerp);
        }

        /// <summary>
        /// Invoked when the state is exited.
        /// </summary>
        public virtual void Exit()
        {
            
        }
    }
}
