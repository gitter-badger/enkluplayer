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
        private ElementSchemaProp<string> _axisProp;
        private ElementSchemaProp<bool> _tooltipProp;

        /// <summary>
        /// Renders lines.
        /// </summary>
        private readonly SliderLineRenderer _renderer;

        /// <summary>
        /// Slider button widget.
        /// </summary>
        private ButtonWidget _handle;

        /// <summary>
        /// Caption widget.
        /// </summary>
        private CaptionWidget _tooltip;

        /// <summary>
        /// Image widgets.
        /// </summary>
        private ImageWidget _minImage;
        private ImageWidget _maxImage;

        /// <summary>
        /// Origin of line.
        /// </summary>
        private Vector3 _O;

        /// <summary>
        /// Direction of normal.
        /// </summary>
        private Vector3 _n;

        /// <summary>
        /// Direction of line.
        /// </summary>
        private Vector3 _d;

        /// <summary>
        /// If true, snaps the button position.
        /// </summary>
        private bool _isDirty = true;
        
        /// <summary>
        /// Value at center.
        /// </summary>
        private float _valueAtCenter;
        
        /// <summary>
        /// Value of the slider.
        /// </summary>
        public float Value
        {
            get
            {
                if (null != _handle)
                {
                    var center = (_maxImage.GameObject.transform.position + _minImage.GameObject.transform.position) / 2f;
                    var handlePos = _handle.GameObject.transform.position;

                    var minDelta = handlePos - _minImage.GameObject.transform.position;
                    var maxDelta = handlePos - _maxImage.GameObject.transform.position;
                    var minMag = minDelta.magnitude;
                    var maxMag = maxDelta.magnitude;

                    if (minMag < maxMag)
                    {
                        return _valueAtCenter - ValueMultiplier * (center - _handle.GameObject.transform.position).magnitude;
                    }

                    return _valueAtCenter + ValueMultiplier * (center - _handle.GameObject.transform.position).magnitude;
                }

                return _valueAtCenter;
            }
            set
            {
                // TODO: Calculate what the value at the center would be given the current state
                _valueAtCenter = value;
            }
        }

        /// <summary>
        /// Multiplies value.
        /// </summary>
        public float ValueMultiplier { get; set; }

        /// <inheritdoc />
        public bool Focused
        {
            get
            {
                if (null != _handle)
                {
                    return _handle.Focused;
                }
                return false;
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
                if (null != _handle)
                {
                    return _handle.GameObject.transform.position.ToVec();
                }

                return GameObject.transform.position.ToVec();
            }
            set
            {
                if (_handle != null)
                {
                    _handle.GameObject.transform.position = value.ToVector();
                }
            }
        }

        /// <inheritdoc />
        public Vec3 FocusScale
        {
            get
            {
                if (null != _handle)
                {
                    return _handle.GameObject.transform.lossyScale.ToVec();
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
            get
            {
                if (null != _handle)
                {
                    return _handle.Aim;
                }
                return 1f;
            }
        }

        /// <inheritdoc />
        public event Action<IInteractable> OnVisibilityChanged;
        
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
            ValueMultiplier = 1f;

            _elements = elements;
            _intentions = intentions;
            _interactions = interactions;

            _renderer = GameObject.AddComponent<SliderLineRenderer>();
            _renderer.enabled = false;
        }

        /// <inheritdoc />
        public bool Raycast(Vec3 origin, Vec3 direction)
        {
            return _handle.Raycast(origin, direction);
        }
        
        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();

            Schema.Get<bool>("visible").OnChanged += (prop, prev, next) => UpdateBasis();

            _lengthProp = Schema.Get<float>("length");
            _lengthProp.OnChanged += Length_OnChanged;

            _axisProp = Schema.Get<string>("axis");
            _axisProp.OnChanged += Axis_OnChanged;

            _tooltipProp = Schema.Get<bool>("tooltip");
            _tooltipProp.OnChanged += Tooltip_OnChanged;

            _handle = (ButtonWidget) _elements.Element("<?Vine><Button id='btn-x' position=(-0.2, 0, 0) ready.color='Highlight' />");
            AddChild(_handle);
            _handle.Activator.OnActivated += MoveSlider_OnActivated;

            _tooltip = (CaptionWidget)_elements.Element("<?Vine><Caption position=(0, 0.1, 0) visible=true label='Placeholder' fontSize=50 width=500.0 alignment='MidCenter' />");
            _handle.AddChild(_tooltip);

            _minImage = (ImageWidget) _elements.Element("<?Vine><Image src='res://Art/Textures/arrow-left' width=0.1 height=0.1 />");
            AddChild(_minImage);

            _maxImage = (ImageWidget) _elements.Element("<?Vine><Image src='res://Art/Textures/arrow-right' width=0.1 height=0.1 />");
            AddChild(_maxImage);
            
            _interactions.Add(this);
            Interactable = true;
            
            _renderer.enabled = true;
            _isDirty = true;
            _valueAtCenter = 0f;

            UpdateBasis();
            UpdateTooltipVisibility();
        }
        
        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            _renderer.enabled = false;

            Interactable = false;
            _interactions.Remove(this);

            _lengthProp.OnChanged -= Length_OnChanged;
            _axisProp.OnChanged -= Axis_OnChanged;

            base.UnloadInternalAfterChildren();
        }

        /// <inheritdoc />
        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            if (!Visible)
            {
                return;
            }

            UpdateArrowPositions();
            UpdateButtonPosition();
            UpdateTooltip();
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

            _isDirty = true;

            if (null != OnVisibilityChanged)
            {
                OnVisibilityChanged(this);
            }
        }

        /// <summary>
        /// Generates a definition of a line.
        /// </summary>
        private void UpdateBasis()
        {
            _O = GameObject.transform.position;

            var flattenedForward = new Vector3(
                _intentions.Forward.x,
                0,
                _intentions.Forward.z).normalized;

            var axis = EnumExtensions.Parse<AxisType>(_axisProp.Value.ToUpperInvariant());
            if (axis == AxisType.Y)
            {
                _d = Vector3.up;
                _n = -flattenedForward;
            }
            else if (axis == AxisType.Z)
            {
                // plane tilted at 45 degrees from flat forward
                var forward = _intentions.Forward.ToVector();
                _d = Quaternion.AngleAxis(-45, Vector3.right) * forward;
                _n = Quaternion.AngleAxis(-135, Vector3.right) * forward;
            }
            else
            {
                _d = new Vector3(
                    _intentions.Right.x,
                    0,
                    _intentions.Right.z).normalized;
                _n = -flattenedForward;
            }

            _renderer.O = _O;
            _renderer.d = _d;
        }

        /// <summary>
        /// Positions the arrows.
        /// </summary>
        private void UpdateArrowPositions()
        {
            var axis = EnumExtensions.Parse<AxisType>(_axisProp.Value.ToUpperInvariant());
            if (axis == AxisType.Y)
            {
                _minImage.Schema.Set("src", "res://Art/Textures/arrow-down");
                _maxImage.Schema.Set("src", "res://Art/Textures/arrow-up");
                _handle.Schema.Set("icon", "arrow-double-vertical");
            }
            else if (axis == AxisType.Z)
            {
                _minImage.Schema.Set("src", "res://Art/Textures/arrow-down");
                _maxImage.Schema.Set("src", "res://Art/Textures/arrow-up");
                _handle.Schema.Set("icon", "arrow-z");
            }
            else
            {
                _minImage.Schema.Set("src", "res://Art/Textures/arrow-left");
                _maxImage.Schema.Set("src", "res://Art/Textures/arrow-right");
                _handle.Schema.Set("icon", "arrow-double");
            }

            _minImage.GameObject.transform.position = _O - _lengthProp.Value * _d;
            _maxImage.GameObject.transform.position = _O + _lengthProp.Value * _d;
        }

        /// <summary>
        /// Updates the button's position.
        /// </summary>
        private void UpdateButtonPosition()
        {
            // calculate the intersection of the intention with the slider plane
            var intersection = CalculateIntentionIntersection();
            
            // project onto the slider line
            var projection = CalculateScalarProjection(_O, _d, intersection);
            
            // position the slider
            var target = _O + projection * _d;
            if (_isDirty)
            {
                _isDirty = false;
            }
            else
            {
                target = Vector3.Lerp(
                    _handle.GameObject.transform.position,
                    target,
                    3f * Time.deltaTime);
            }

            var handle = Render.Handle("IUX");
            if (null != handle)
            {
                handle.Draw(ctx =>
                {
                    ctx.Color(UnityEngine.Color.green);
                    ctx.Cube(intersection, 0.1f);
                    ctx.Line(intersection, _O + projection * _d);
                    ctx.Line(_O, _O + projection * _d);

                    ctx.Color(UnityEngine.Color.yellow);
                    ctx.Cube(_O, 0.1f);

                    ctx.Color(UnityEngine.Color.blue);
                    ctx.Line(_O, _O + _d);
                    ctx.Color(UnityEngine.Color.red);
                    ctx.Line(_O, _O + _n);
                });
            }

            _handle.GameObject.transform.position = target;
        }

        /// <summary>
        /// Updates the tooltip.
        /// </summary>
        private void UpdateTooltip()
        {
            _tooltip.Label = Value.ToString();
        }

        /// <summary>
        /// Calculates the intention forward intersection with the plane the
        /// slider is on.
        /// </summary>
        /// <returns></returns>
        private Vector3 CalculateIntentionIntersection()
        {
            // Line: P = O + td
            // Plane: (P - P0) * n = 0

            // plane
            var P0 = _O;

            // line
            var O = _intentions.Origin.ToVector();
            var d = _intentions.Forward.ToVector();

            // substitute plane equation in for P to find t
            var t = Vector3.Dot(P0 - O, _n) / Vector3.Dot(_n, d);

            // return intersection
            var intersection = O + t * d;

            var handle = Render.Handle("IUX");
            if (null != handle)
            {
                handle.Draw(ctx =>
                {
                    ctx.Color(UnityEngine.Color.blue);
                    ctx.Line(_O, _O + _n);
                });
            }

            return intersection;
        }

        /// <summary>
        /// Calculates the scalar projection of P onto the line definted by O + td.
        /// </summary>
        private float CalculateScalarProjection(Vector3 O, Vector3 d, Vector3 P)
        {
            return Vector3.Dot(P - O, d);
        }

        /// <summary>
        /// Updates the tooltip's visibility based on prop.
        /// </summary>
        private void UpdateTooltipVisibility()
        {
            _tooltip.Schema.Set("visible", _tooltipProp.Value);
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
        /// <param name="prop">The property.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Tooltip_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            UpdateTooltipVisibility();
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
    }
}