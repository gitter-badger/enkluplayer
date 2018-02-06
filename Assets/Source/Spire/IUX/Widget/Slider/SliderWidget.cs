using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic slider control.
    /// </summary>
    public class SliderWidget : Widget
    {
        /// <summary>
        /// For creating elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Manages intention.
        /// </summary>
        private readonly IIntentionManager _intentions;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<float> _lengthProp;
        private ElementSchemaProp<float> _radiusProp;
        private ElementSchemaProp<float> _sizeMaxProp;
        private ElementSchemaProp<float> _sizeMinProp;

        /// <summary>
        /// Image widget.
        /// </summary>
        private ImageWidget _image;

        /// <summary>
        /// Normalized value.
        /// </summary>
        public float Value { get; set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public SliderWidget(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IElementFactory elements,
            IIntentionManager intentions)
            : base(
                gameObject,
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _elements = elements;
            _intentions = intentions;
        }

        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            _sizeMaxProp = Schema.Get<float>("size.max");
            _sizeMinProp = Schema.Get<float>("size.min");
            _radiusProp = Schema.Get<float>("radius");
            _lengthProp = Schema.Get<float>("length");
            _lengthProp.OnChanged += Length_OnChanged;

            _image = (ImageWidget) _elements.Element("<Image src='res://Art/Textures/Outer Gradient' />");
            AddChild(_image);
        }

        protected override void AfterUnloadChildrenInternal()
        {
            base.AfterUnloadChildrenInternal();

            _lengthProp.OnChanged -= Length_OnChanged;
        }

        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            var aim = CalculateAim();
            _image.GameObject.transform.localScale = 
                (_sizeMinProp.Value + (1 - aim) * (_sizeMaxProp.Value - _sizeMinProp.Value))
                * Vector3.one;
        }

        private void Length_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            
        }

        /// <summary>
        /// Updates the aim as a function of focus towards the center of the
        /// image.
        /// </summary>
        private float CalculateAim()
        {
            var eyePosition = _intentions.Origin;
            var eyeDirection = _intentions.Forward;
            var delta = GameObject.transform.position.ToVec() - eyePosition;
            var directionToButton = delta.Normalized;

            var eyeDistance = delta.Magnitude;
            var radius = _radiusProp.Value;
            var maxTheta = Mathf.Atan2(radius, eyeDistance);

            var cosTheta = Vec3.Dot(
                directionToButton,
                eyeDirection);
            var theta = Mathf.Approximately(cosTheta, 1.0f)
                ? 0.0f
                : Mathf.Acos(cosTheta);

            return Mathf.Approximately(maxTheta, 0.0f)
                ? 0.0f
                : 1.0f - Mathf.Clamp01(Mathf.Abs(theta / maxTheta));
        }
    }
}