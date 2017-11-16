using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Visual characteristics common to all buttons.
    /// </summary>
    public class ButtonState : Element, IState
    {
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<float> _propFrameScale;
        private ElementSchemaProp<int> _propFrameColor;
        private ElementSchemaProp<int> _propCaptionColor;
        private ElementSchemaProp<int> _propTween;

        /// <summary>
        /// Affected button.
        /// </summary>
        private Button _button;

        /// <summary>
        /// Affected button.
        /// </summary>
        public Button Button { get { return _button; } }

        /// <summary>
        /// Color of the button during this state.
        /// </summary>
        public VirtualColor FrameColor { get { return (VirtualColor)_propFrameScale.Value; } }

        /// <summary>
        /// Color of the button during this state.
        /// </summary>
        public VirtualColor CaptionColor { get { return (VirtualColor)_propCaptionColor.Value; } }

        /// <summary>
        /// Color of the button during this state.
        /// </summary>
        public float FrameScale { get { return _propFrameColor.Value; } }

        /// <summary>
        /// Tween into this state.
        /// </summary>
        public TweenType Tween { get { return (TweenType)_propTween.Value; } }

        /// <summary>
        /// Setup.
        /// </summary>
        /// <param name="button"></param>
        public void Initialize(Button button)
        {
            _button = button;
        }

        /// <summary>
        /// Prop Initialization
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _propFrameColor = Schema.Get<int>("frameColor");
            _propCaptionColor = Schema.Get<int>("captionColor");
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
            var tweenDuration = Button.Tweens.DurationSeconds(Tween);
            var tweenLerp
                = tweenDuration > Mathf.Epsilon
                    ? deltaTime / tweenDuration
                    : 1.0f;

            // blend the frame's color.
            var frame = Button.Activator.Frame;
            var frameColor = Button.Colors.GetColor(FrameColor);
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

            // blend the caption's color.
            var captionColor = Button.Colors.GetColor(CaptionColor);
            var caption = Button.Caption;
            if (caption != null)
            {
                caption.LocalColor
                    = Col4.Lerp(
                        caption.LocalColor,
                        captionColor,
                        tweenLerp);
            }
        }

        /// <summary>
        /// Invoked when the state is exited.
        /// </summary>
        public virtual void Exit()
        {
            
        }
    }
}
