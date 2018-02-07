using System;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Basic slider control.
    /// </summary>
    public class SliderWidget : Widget, IInteractable
    {
        /// <summary>
        /// Defines the type of axes.
        /// </summary>
        public enum AxisType
        {
            X,
            Y,
            Z,
            Custom
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

        /// <summary>
        /// Start of slider, in world space.
        /// </summary>
        private Vector3 _p0;

        /// <summary>
        /// End of slider, in world space.
        /// </summary>
        private Vector3 _p1;

        /// <summary>
        /// Normalized value.
        /// </summary>
        public float Value { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
        
        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool Interactable { get; private set; }

        /// <inheritdoc />
        public int HighlightPriority { get; set; }

        /// <inheritdoc />
        public bool IsHighlighted { get; set; }

        /// <inheritdoc />
        public float Aim
        {
            get { return 1f; }
        }

        /// <inheritdoc />
        public event Action<IInteractable> OnVisibilityChanged;

        /// <summary>
        /// Called when the control is not being focused on.
        /// </summary>
        public event Action OnUnfocused;

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

        /// <inheritdoc />
        public bool Raycast(Vec3 origin, Vec3 direction)
        {
            return true;
        }

        protected override void BeforeLoadChildrenInternal()
        {
            base.BeforeLoadChildrenInternal();

            _sizeMaxProp = Schema.Get<float>("size.max");
            _sizeMinProp = Schema.Get<float>("size.min");
            _radiusProp = Schema.Get<float>("radius");

            _lengthProp = Schema.Get<float>("length");
            _lengthProp.OnChanged += Length_OnChanged;

            _axisProp = Schema.Get<string>("axis");
            _axisProp.OnChanged += Axis_OnChanged;

            _image = (ImageWidget)_elements.Element("<Image src='res://Art/Textures/Outer Gradient' />");
            AddChild(_image);

            _interactions.Add(this);
            Interactable = true;
        }
        
        /// <inheritdoc />
        protected override void AfterUnloadChildrenInternal()
        {
            base.AfterUnloadChildrenInternal();

            Interactable = false;
            _interactions.Remove(this);

            _lengthProp.OnChanged -= Length_OnChanged;
            _axisProp.OnChanged -= Axis_OnChanged;
        }

        /// <inheritdoc />
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

            if (Math.Abs(aim) < float.Epsilon)
            {
                if (null != OnUnfocused)
                {
                    OnUnfocused();
                }
            }
        }

        /// <inheritdoc />
        protected override void OnVisibilityUpdated()
        {
            base.OnVisibilityUpdated();

            if (Visible)
            {
                _interactions.Add(this);
                Interactable = true;
            }
            else
            {
                Interactable = false;
                _interactions.Remove(this);
            }

            if (null != OnVisibilityChanged)
            {
                OnVisibilityChanged(this);
            }
        }

        /// <summary>
        /// Renders debugging information.
        /// </summary>
        private void DebugRender()
        {
            var handle = Render.Handle("IUX");
            if (null != handle)
            {
                handle.Draw(ctx =>
                {
                    ctx.Color(UnityEngine.Color.green);
                    ctx.Line(_p0, _p1);
                });
            }
        }

        /// <summary>
        /// Recalculates the plane to restrict slider controls to.
        /// </summary>
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

            _p0 = position - _lengthProp.Value * offset;
            _p1 = position + _lengthProp.Value * offset;
        }

        /// <summary>
        /// Updates the image position.
        /// </summary>
        private void UpdatePosition()
        {
            // generate plane normal
            var segmentDirection = (_p1 - _p0).normalized;
            var segmentUp = Vector3
                .Cross(_intentions.Forward.ToVector(), segmentDirection)
                .normalized;
            var n = -Vector3
                .Cross(segmentDirection, segmentUp)
                .normalized;

            var p3 = _p0;
            var p1 = _intentions.Origin.ToVector();
            var p2 = p1 + _intentions.Forward.ToVector();
            var p3subp1 = p3 - p1;
            var p2subp1 = p2 - p1;
            var n_dot_p3subp1 = Vector3.Dot(n, p3subp1);
            var n_dot_p2subp1 = Vector3.Dot(n, p2subp1);

            // intersect ray with plane
            var t = n_dot_p3subp1 / n_dot_p2subp1;
            var intersection = _intentions.Origin + _intentions.Forward * t;

            Value = CalculateValue(intersection.ToVector(), _p0, _p1);
            
            _image.Schema.Set("position", Vector3.Lerp(_p0, _p1, Value).ToVec());
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

        /// <summary>
        /// Called when property changes.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Length_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            RecalculatePlane();
        }

        /// <summary>
        /// Called when property changes.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Axis_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            RecalculatePlane();
        }
    }
}