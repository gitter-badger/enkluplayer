﻿using System;
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
        private ElementSchemaProp<string> _axisProp;
        private ElementSchemaProp<bool> _tooltipProp;

        /// <summary>
        /// Renders lines.
        /// </summary>
        private readonly SliderLineRenderer _renderer;

        /// <summary>
        /// Slider button widget.
        /// </summary>
        private ButtonWidget _moveSlider;

        /// <summary>
        /// Caption widget.
        /// </summary>
        private CaptionWidget _annotation;

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
        /// Direction of line.
        /// </summary>
        private Vector3 _d;

        /// <summary>
        /// If true, snaps the button position.
        /// </summary>
        private bool _isDirty = true;
        
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

            _renderer = GameObject.AddComponent<SliderLineRenderer>();
            _renderer.enabled = false;
        }

        /// <inheritdoc />
        public bool Raycast(Vec3 origin, Vec3 direction)
        {
            return true;
        }
        
        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();
            
            _lengthProp = Schema.Get<float>("length");
            _lengthProp.OnChanged += Length_OnChanged;

            _axisProp = Schema.Get<string>("axis");
            _axisProp.OnChanged += Axis_OnChanged;

            _tooltipProp = Schema.Get<bool>("tooltip");
            _tooltipProp.OnChanged += Tooltip_OnChanged;

            _moveSlider = (ButtonWidget) _elements.Element("<?Vine><Button id='btn-x' icon='arrow-double' position=(-0.2, 0, 0) ready.color='Highlight' />");
            AddChild(_moveSlider);
            _moveSlider.Activator.OnActivated += MoveSlider_OnActivated;

            _annotation = (CaptionWidget)_elements.Element("<?Vine><Caption id='value-annotation' position=(0, 0.1, 0) visible=true label='Placeholder' fontSize=50 width=500.0 alignment='MidCenter' />");
            _moveSlider.AddChild(_annotation);

            _minImage = (ImageWidget) _elements.Element("<?Vine><Image src='res://Art/Textures/arrow-left' width=0.1 height=0.1 />");
            AddChild(_minImage);

            _maxImage = (ImageWidget) _elements.Element("<?Vine><Image src='res://Art/Textures/arrow-right' width=0.1 height=0.1 />");
            AddChild(_maxImage);
            
            _interactions.Add(this);
            Interactable = true;
            
            _renderer.enabled = true;
            _isDirty = true;

            UpdateTooltipVisibility();
        }
        
        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            _renderer.enabled = false;

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
                return;
            }

            UpdateBasis();
            UpdateArrowPositions();
            UpdateButtonPosition();
            UpdateValue();

            var handle = Render.Handle("IUX");
            if (null != handle)
            {
                handle.Draw(ctx =>
                {
                    ctx.Color(UnityEngine.Color.yellow);
                    ctx.Cube(_O, 0.1f);
                    ctx.Line(_O, _O + _d);
                });
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

            var axis = EnumExtensions.Parse<AxisType>(_axisProp.Value.ToUpperInvariant());
            if (axis == AxisType.X)
            {
                _d = new Vector3(
                    _intentions.Right.x,
                    0,
                    _intentions.Right.z).normalized;
            }
            else
            {
                _d = Vector3.up;
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
            }
            else
            {
                _minImage.Schema.Set("src", "res://Art/Textures/arrow-left");
                _maxImage.Schema.Set("src", "res://Art/Textures/arrow-right");
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
                    _moveSlider.GameObject.transform.position,
                    target,
                    3f * Time.deltaTime);
            }

            _moveSlider.GameObject.transform.position = target;
        }

        /// <summary>
        /// Updates the value + label.
        /// </summary>
        private void UpdateValue()
        {
            _annotation.Label = Value.ToString();
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
            var n = -_intentions.Forward.ToVector().normalized;

            // line
            var O = _intentions.Origin.ToVector();
            var d = _intentions.Forward.ToVector();

            // substitute plane equation in for P to find t
            var t = Vector3.Dot(P0 - O, n) / Vector3.Dot(n, d);

            // return intersection
            return O + t * d;
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
            _annotation.Schema.Set("visible", _tooltipProp.Value);
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