using RTEditor;
using System;
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
        /// Slider button widget.
        /// </summary>
        private ButtonWidget _moveSlider;
        /// <summary>
        /// Caption widget.
        /// </summary>
        private CaptionWidget _Annotation;
        /// <summary>
        /// Image widgets.
        /// </summary>
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
                if (null != _moveSlider)
                {
                    return _moveSlider.GameObject.transform.position.ToVec();
                }

                return GameObject.transform.position.ToVec();
            }
        }
        
        /// <inheritdoc />
        public Vec3 FocusScale
        {
            get
            {
                if (null != _moveSlider)
                {
                    return _moveSlider.GameObject.transform.lossyScale.ToVec();
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
        /// Called when the control is not being focused on.
        /// </summary>
        public event Action OnSliderValueConfirmed;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SliderWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IInteractionManager interactions,
            IElementFactory elements,
            IIntentionManager intentions)
            : base(
                gameObject,
                layers,
                tweens,
                colors)
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

        /// <summary>
        /// Calculates two extreme points based on axis.
        /// </summary>
        public Vector3[] CalculatePivotPoints()
        {
            var axis = EnumExtensions.Parse<AxisType>(_axisProp.Value.ToUpperInvariant());
            var offset = _intentions.Right;
            Vector3 p0, p1;
            if (axis == AxisType.X)
            {
                offset = _intentions.Right;
                p0 = new Vector3(GameObject.transform.position.x - 2f, GameObject.transform.position.y, GameObject.transform.position.z);
                p1 = new Vector3(p0.x + 4f, p0.y, p0.z);
            }
            else
            {
                offset = _intentions.Up;
                p0 = new Vector3(GameObject.transform.position.x, GameObject.transform.position.y - 2f, GameObject.transform.position.z);
                p1 = new Vector3(p0.x, p0.y + 4f, p0.z);
            }
            Vector3 adjustedp0 = p0 - _lengthProp.Value * offset.ToVector();
            Vector3 adjustedp1 = p1 + _lengthProp.Value * offset.ToVector();
            return new Vector3[] { adjustedp0, adjustedp1 };
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();
           
            _sizeMaxProp = Schema.Get<float>("size.max");
            _sizeMinProp = Schema.Get<float>("size.min");
            _radiusProp = Schema.Get<float>("radius");

            _lengthProp = Schema.Get<float>("length");
            _lengthProp.OnChanged += Length_OnChanged;

            _axisProp = Schema.Get<string>("axis");
            _axisProp.OnChanged += Axis_OnChanged;

            _moveSlider = (ButtonWidget) _elements.Element("<?Vine><Button id='btn-x' icon='arrow-double' position=(-0.2, 0, 0) ready.color='Highlight' />");
            AddChild(_moveSlider);
            _moveSlider.Activator.OnActivated += MoveSlider_OnActivated;

            _Annotation = (CaptionWidget)_elements.Element("<?Vine><Caption id='value-annotation' position=(0, 0.1, 0) visible=true label='Placeholder' fontSize=50 width=500.0 alignment='MidCenter' />");
            _moveSlider.AddChild(_Annotation);

            _minImage = (ImageWidget) _elements.Element("<?Vine><Image src='res://Art/Textures/arrow-left' width=0.1 height=0.1 />");
            AddChild(_minImage);

            _maxImage = (ImageWidget) _elements.Element("<?Vine><Image src='res://Art/Textures/arrow-right' width=0.1 height=0.1 />");
            AddChild(_maxImage);
            
            _interactions.Add(this);
            Interactable = true;
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

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
            _moveSlider.GameObject.transform.localScale = scalar * Vector3.one;
            
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
            Vector3[] pivotPoints = CalculatePivotPoints();
            Value = CalculateValue(intersection, pivotPoints[0], pivotPoints[1]);

            // force world position, not position property
            _moveSlider.GameObject.transform.position = Vector3.Lerp(pivotPoints[0], pivotPoints[1], Value);
            _Annotation.Label = Value.ToString();
        }

        /// <summary>
        /// Recalculates the plane to restrict slider controls to.
        /// </summary>
        private void CalculatePlane()
        {
            var axis = EnumExtensions.Parse<AxisType>(_axisProp.Value.ToUpperInvariant());

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
        /// Called when property changes.
        /// </summary>
        private void MoveSlider_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnSliderValueConfirmed)
            {
                OnSliderValueConfirmed();
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
    }
}