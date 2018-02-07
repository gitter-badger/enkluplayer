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
        private ImageWidget _positionImage;
        private ImageWidget _minImage;
        private ImageWidget _maxImage;

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
                if (null != _positionImage)
                {
                    return _positionImage.GameObject.transform.position.ToVec();
                }

                return GameObject.transform.position.ToVec();
            }
        }
        
        /// <inheritdoc />
        public Vec3 FocusScale
        {
            get
            {
                if (null != _positionImage)
                {
                    return _positionImage.GameObject.transform.lossyScale.ToVec();
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

        /// <inheritdoc />
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

            _positionImage = (ImageWidget) _elements.Element("<Image src='res://Art/Textures/Outer Gradient' />");
            AddChild(_positionImage);

            _minImage = (ImageWidget) _elements.Element("<Image src='res://Art/Textures/arrow-left' />");
            AddChild(_minImage);

            _maxImage = (ImageWidget) _elements.Element("<Image src='res://Art/Textures/arrow-right' />");
            AddChild(_maxImage);
            
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

            if (!Visible)
            {
                Value = 0.5f;
                return;
            }

            CalculatePlane();
            var intersection = CalculateIntentionIntersection();

            UpdatePosition(intersection);
            var aim = CalculateAim(intersection);
            var scalar = _sizeMinProp.Value + (1 - aim) * (_sizeMaxProp.Value - _sizeMinProp.Value);
            _positionImage.GameObject.transform.localScale = scalar * Vector3.one;
            
            if (Math.Abs(aim) < float.Epsilon)
            {
                if (null != OnUnfocused)
                {
                    OnUnfocused();
                }
            }

            DebugRender();
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
        /// Updates the image position.
        /// </summary>
        private void UpdatePosition(Vector3 intersection)
        {
            Value = CalculateValue(intersection, _p0, _p1);

            // force world position, not position property
            _positionImage.GameObject.transform.position = Vector3.Lerp(_p0, _p1, Value);
        }

        /// <summary>
        /// Recalculates the plane to restrict slider controls to.
        /// </summary>
        private void CalculatePlane()
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

            var offset = _intentions.Right;
            if (axis == AxisType.X)
            {
                offset = _intentions.Right;

                _minImage.Schema.Set("src", "res://Art/Textures/arrow-left");
                _maxImage.Schema.Set("src", "res://Art/Textures/arrow-right");
            }
            else if (axis == AxisType.Y)
            {
                offset = _intentions.Up;

                _minImage.Schema.Set("src", "res://Art/Textures/arrow-down");
                _maxImage.Schema.Set("src", "res://Art/Textures/arrow-up");
            }
            
            var position = GameObject.transform.position;
            
            _p0 = position - _lengthProp.Value * offset.ToVector();
            _p1 = position + _lengthProp.Value * offset.ToVector();

            _minImage.GameObject.transform.position = _p0;
            _maxImage.GameObject.transform.position = _p1;
        }

        /// <summary>
        /// Calculates the intention forward intersection with the plane made
        /// from the points on slider + up.
        /// </summary>
        /// <returns></returns>
        private Vector3 CalculateIntentionIntersection()
        {
            // generate plane normal
            var right = (_p1 - _p0).normalized;
            var up = Vector3
                .Cross(_intentions.Forward.ToVector(), right)
                .normalized;
            var normal = -Vector3
                .Cross(right, up)
                .normalized;

            var p3 = _p0;
            var p1 = _intentions.Origin.ToVector();
            var p2 = p1 + _intentions.Forward.ToVector();

            var s = p3 - p1;
            var r = p2 - p1;
            var n_dot_s = Vector3.Dot(normal, s);
            var n_dot_r = Vector3.Dot(normal, r);

            // intersect ray with plane
            var t = n_dot_s / n_dot_r;
            var intersection = _intentions.Origin + _intentions.Forward * t;

            if (float.IsNaN(intersection.x))
            {
                throw new Exception("Invalid.");
            }

            return intersection.ToVector();
        }

        /// <summary>
        /// Updates the aim as a function of focus towards the center of the
        /// line segment.
        /// </summary>
        private float CalculateAim(Vector3 intersection)
        {
            var distance = Vector3.Magnitude(Focus.ToVector() - intersection);
            var rad = _radiusProp.Value;

            return Mathf.Clamp01(1 - distance / rad);
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
            //
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
            //
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
    }
}