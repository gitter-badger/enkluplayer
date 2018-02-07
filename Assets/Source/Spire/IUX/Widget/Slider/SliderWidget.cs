using System;
using System.Resources;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic slider control.
    /// </summary>
    public class SliderWidget : Widget, IInteractable
    {
        public enum AxisType
        {
            X,
            Y,
            Z
        }

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IElementFactory _elements;
        private readonly IIntentionManager _intentions;
        private readonly IInteractionManager _interactions;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<float> _lengthProp;
        private ElementSchemaProp<float> _radiusProp;
        private ElementSchemaProp<float> _sizeMaxProp;
        private ElementSchemaProp<float> _sizeMinProp;
        private ElementSchemaProp<string> _axisProp;

        /// <summary>
        /// Image widget.
        /// </summary>
        private ImageWidget _image;

        private Vector3 P0;
        private Vector3 P1;

        /// <summary>
        /// Normalized value.
        /// </summary>
        public float Value { get; set; }

        public bool Focused
        {
            get
            {
                return true;
            }
            set
            {
                //
            }
        }

        public Vec3 Focus
        {
            get
            {
                if (null != _image)
                {
                    return _image.GameObject.transform.position.ToVec();
                }

                return GameObject.transform.position.ToVec();
            }
        }

        public Vec3 FocusScale
        {
            get
            {
                if (null != _image)
                {
                    return _image.GameObject.transform.lossyScale.ToVec();
                }

                return GameObject.transform.lossyScale.ToVec();
            }
        }

        public bool Interactable { get; private set; }
        public int HighlightPriority { get; set; }
        public bool IsHighlighted { get; set; }

        public float Aim
        {
            get { return 1f; }
        }

        public event Action<IInteractable> OnVisibilityChanged;

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
            IInteractionManager interactions,
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
            _interactions = interactions;
        }

        public bool Raycast(Vec3 origin, Vec3 direction)
        {
            return true;
        }

        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            _sizeMaxProp = Schema.Get<float>("size.max");
            _sizeMinProp = Schema.Get<float>("size.min");
            _radiusProp = Schema.Get<float>("radius");

            _lengthProp = Schema.Get<float>("length");
            _lengthProp.OnChanged += Length_OnChanged;

            _axisProp = Schema.Get<string>("axis");
            _axisProp.OnChanged += Axis_OnChanged;
            
            _image = (ImageWidget) _elements.Element("<Image src='res://Art/Textures/Outer Gradient' />");
            AddChild(_image);

            _interactions.Add(this);
            Interactable = true;
        }
        
        protected override void AfterUnloadChildrenInternal()
        {
            base.AfterUnloadChildrenInternal();

            Interactable = false;
            _interactions.Remove(this);

            _lengthProp.OnChanged -= Length_OnChanged;
            _axisProp.OnChanged -= Axis_OnChanged;
        }

        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            var aim = CalculateAim();
            _image.GameObject.transform.localScale = 
                (_sizeMinProp.Value + (1 - aim) * (_sizeMaxProp.Value - _sizeMinProp.Value))
                * Vector3.one;

            RecalculatePlane();

            UpdatePosition();
            DebugRender();
        }

        private void DebugRender()
        {
            var handle = Render.Handle("IUX");
            if (null != handle)
            {
                handle.Draw(ctx =>
                {
                    ctx.Color(UnityEngine.Color.green);
                    ctx.Line(P0, P1);
                });
            }
        }

        private void RecalculatePlane()
        {
            var axis = AxisType.X;
            try
            {
                axis = (AxisType) Enum.Parse(
                    typeof(AxisType),
                    _axisProp.Value.ToUpperInvariant());
            }
            catch
            {
                //
            }

            var offset = Vector3.right;
            if (axis == AxisType.Y)
            {
                offset = Vector3.up;
            }

            if (axis == AxisType.Z)
            {
                offset = Vector3.forward;
            }

            var position = GameObject.transform.position;

            P0 = position - _lengthProp.Value * offset;
            P1 = position + _lengthProp.Value * offset;
        }

        private void UpdatePosition()
        {
            // generate plane normal
            var segmentDirection = (P1 - P0).normalized;
            var segmentUp = Vector3
                .Cross(_intentions.Forward.ToVector(), segmentDirection)
                .normalized;
            var n = -Vector3
                .Cross(segmentDirection, segmentUp)
                .normalized;

            var p3 = P0;
            var p1 = _intentions.Origin.ToVector();
            var p2 = p1 + _intentions.Forward.ToVector();
            var p3subp1 = p3 - p1;
            var p2subp1 = p2 - p1;
            var n_dot_p3subp1 = Vector3.Dot(n, p3subp1);
            var n_dot_p2subp1 = Vector3.Dot(n, p2subp1);

            // intersect ray with plane
            var t = n_dot_p3subp1 / n_dot_p2subp1;
            var intersection = _intentions.Origin + _intentions.Forward * t;

            Value = CalculateValue(intersection.ToVector(), P0, P1);
            
            _image.Schema.Set("position", Vector3.Lerp(P0, P1, Value).ToVec());
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

        /// <summary>
        /// Calculates the value.
        /// </summary>
        private float CalculateValue(Vector3 p, Vector3 s0, Vector3 s1)
        {
            var s1subs0 = s1 - s0;
            var psubs0 = p - s0;
            var s1subs0_dot_psubs0 = Vector3.Dot(s1subs0.normalized, psubs0.normalized);
            var psubs0_mag = psubs0.magnitude;
            var q_mag = psubs0_mag * s1subs0_dot_psubs0;
            var s1subs0_magnitude = s1subs0.magnitude;
            var value = s1subs0_magnitude > Mathf.Epsilon
                ? Mathf.Clamp01(q_mag / s1subs0_magnitude)
                : 1;

            return value;
        }

        private void Length_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            RecalculatePlane();
        }

        private void Axis_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            RecalculatePlane();
        }
    }
}