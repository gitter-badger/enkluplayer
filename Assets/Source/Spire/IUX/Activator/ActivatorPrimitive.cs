using System;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Holds an activator.
    /// </summary>
    public class ActivatorPrimitive : Widget, IInteractable
    {
        public enum ActivationType
        {
            Fill,
            Scale
        }

        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IInteractionManager _interaction;
        private readonly IIntentionManager _intention;
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Target widget.
        /// </summary>
        private readonly Widget _target;

        /// <summary>
        /// True iff we are registered with <c>IInteractableManager</c>.
        /// </summary>
        private bool _interactable = false;

        /// <summary>
        /// Renders activator.
        /// </summary>
        private ActivatorRenderer _renderer;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<bool> _propInteractionEnabled;
        private ElementSchemaProp<bool> _propHighlighted;
        private ElementSchemaProp<int> _propHighlightPriority;
        private ElementSchemaProp<string> _propActivationType;
        private ElementSchemaProp<bool> _propSliderBehavior;

        /// <summary>
        /// State management for the button
        /// </summary>
        private FiniteStateMachine _states;

        /// <summary>ra
        /// Backing variable for Focused property.
        /// </summary>
        private bool _focused;

        /// <summary>
        /// The icon.
        /// </summary>
        private Sprite _icon;
        
        /// <inheritdoc />
        public bool Interactable
        {
            get
            {
                const float FOCUSABLE_THRESHOLD = 0.99f;
                return _interactable && Visible && Tween > FOCUSABLE_THRESHOLD
                       && InteractionEnabled
                       && (!_interaction.IsOnRails || _propHighlighted.Value);
            }
        }

        /// <inheritdoc />
        public virtual bool Focused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused == value)
                {
                    return;
                }
                
                _focused = value;

                if (!_interactable)
                {
                    return;
                }

                if (_focused)
                {
                    _messages.Publish(
                        MessageTypes.WIDGET_FOCUS,
                        new WidgetFocusEvent(_target));
                }
                else
                {
                    _messages.Publish(
                        MessageTypes.WIDGET_UNFOCUS,
                        new WidgetUnfocusEvent(_target));
                }
            }
        }

        /// <inheritdoc />
        public Vec3 Focus
        {
            get { return GameObject.transform.position.ToVec(); }
        }

        /// <inheritdoc />
        public Vec3 FocusScale
        {
            get { return GameObject.transform.lossyScale.ToVec(); }
        }

        /// <inheritdoc />
        public int HighlightPriority
        {
            get { return _propHighlightPriority.Value; }
            set { _propHighlightPriority.Value = value; }
        }

        /// <inheritdoc />
        public bool IsHighlighted { get; set; }

        /// <inheritdoc />
        public float Aim { get; set; }

        /// <summary>
        /// If true, can be interacted with.
        /// </summary>
        public bool InteractionEnabled
        {
            get { return _propInteractionEnabled.Value; }
            set { _propInteractionEnabled.Value = value; }
        }
        
        /// <summary>
        /// (IUX PATENT)
        /// A scalar percentage [0..1] representing activation completion.
        /// </summary>
        public float Activation { get; set; }

        /// <summary>
        /// (IUX PATENT)
        /// A scalar percentage [0..1] representing targeting steadiness.
        /// 0 = low steadiness -> may be moving over on way to something else.
        /// 1 - high steadiness -> definitely stationary over this.
        /// </summary>
        public float Stability { get; set; }

        /// <summary>
        /// True iff Aim is enabled.
        /// </summary>
        public bool AimEnabled { get; set; }

        /// <summary>
        /// Current State Accessor.
        /// </summary>
        public ActivatorState CurrentState
        {
            get
            {
                return (ActivatorState) _states.Current;
            }
        }

        /// <summary>
        /// Gets/sets the icon.
        /// </summary>
        public Sprite Icon
        {
            get { return _icon; }
            set
            {
                if (value == _icon)
                {
                    return;
                }

                _icon = value;

                SetIcon();
            }
        }

        /// <summary>
        /// Gets/sets icon scale.
        /// </summary>
        public float IconScale
        {
            get { return _renderer.Icon.transform.localScale.x; }
            set { _renderer.Icon.transform.localScale = value * Vector3.one; }
        }
        
        /// <summary>
        /// Invoked when the activator is activated.
        /// </summary>
        public event Action<ActivatorPrimitive> OnActivated;

        /// <inheritdoc />
        public event Action<IInteractable> OnVisibilityChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActivatorPrimitive(
            WidgetConfig config,
            IInteractionManager interaction,
            IIntentionManager intention,
            IMessageRouter messages,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            Widget target)
            : base(
                new GameObject("Activator"),
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _config = config;
            _interaction = interaction;
            _intention = intention;
            _messages = messages;
            _target = target;
        }

        /// <inheritdoc cref="IRaycaster"/>
        public bool Raycast(Vec3 origin, Vec3 direction)
        {
            if (_renderer.FocusCollider != null)
            {
                var ray = new Ray(origin.ToVector(), direction.ToVector());
                RaycastHit hitInfo;
                if (_renderer.FocusCollider.Raycast(ray, out hitInfo, float.PositiveInfinity))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc cref="Element"/>
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            _renderer = Object.Instantiate(
                _config.Activator,
                Vector3.zero,
                Quaternion.identity);
            _renderer.transform.SetParent(GameObject.transform, false);
            _renderer.Initialize(this, _config, Layers, Tweens, Colors, Messages, _intention, _interaction);

            SetIcon();

            AimEnabled = true;

            InitializeProps();
            InitializeStates();

            _interactable = true;
            _interaction.Add(this);
        }

        /// <inheritdoc cref="Element"/>
        protected override void AfterUnloadChildrenInternal()
        {
            _interaction.Remove(this);
            _interactable = false;

            Object.Destroy(_renderer.gameObject);

            base.AfterUnloadChildrenInternal();
        }

        /// <inheritdoc cref="Element"/>
        protected override void UpdateInternal()
        {
            base.UpdateInternal();
            
            var deltaTime = Time.smoothDeltaTime;

            if (!Focused
                || !Interactable
                || !AimEnabled)
            {
                Aim = 0.0f;
                Stability = 0.0f;
            }
            else
            {
                UpdateAim();
                UpdateStability(deltaTime);
            }

            _states.Update(deltaTime);

            DebugDraw();
        }

        /// <inheritdoc />
        protected override void OnVisibilityUpdated()
        {
            base.OnVisibilityUpdated();

            if (null != OnVisibilityChanged)
            {
                OnVisibilityChanged(this);
            }
        }

        /// <summary>
        /// Moves into ready state.
        /// </summary>
        public void Ready()
        {
            _states.Change<ActivatorReadyState>();
        }

        /// <summary>
        /// Moves into activating state.
        /// </summary>
        public void Activating()
        {
            _states.Change<ActivatorActivatingState>();
        }

        /// <summary>
        /// Forced activation.
        /// </summary>
        public void Activate()
        {
            _states.Change<ActivatorActivatedState>();

            Activation = 0;
            
            if (OnActivated != null)
            {
                OnActivated(this);
            }
        }

        /// <summary>
        /// Initializes needed props.
        /// </summary>
        private void InitializeProps()
        {
            _propHighlighted = Schema.Get<bool>("highlighted");
            _propHighlightPriority = Schema.Get<int>("highlightPriority");
            _propInteractionEnabled = Schema.GetOwn<bool>("interactionEnabled", true);

            _propActivationType = Schema.GetOwn("activation.type", ActivationType.Fill.ToString());
            try
            {
                _renderer.Activation = (ActivationType) Enum.Parse(typeof(ActivationType), _propActivationType.Value);
            }
            catch
            {
                // default to fill
            }

            _propSliderBehavior = Schema.GetOwn("activation.slider", false);
        }

        /// <summary>
        /// Initializes sub-states.
        /// </summary>
        private void InitializeStates()
        {
            if (_propSliderBehavior.Value)
            {
                _states = new FiniteStateMachine(new IState[]
                {
                    new ActivatorActivatingState(this, _intention, Schema, false)
                });
                _states.Change<ActivatorActivatingState>();
            }
            else
            {
                _states = new FiniteStateMachine(new IState[]
                {
                    new ActivatorReadyState(this, Schema),
                    new ActivatorActivatingState(this, _intention, Schema, true),
                    new ActivatorActivatedState(_target, this, _messages, Schema)
                });
                _states.Change<ActivatorReadyState>();
            }
        }

        /// <summary>
        /// Updates the aim as a function of focus towards the center of the widget.
        /// </summary>
        private void UpdateAim()
        {
            var eyePosition = _intention.Origin;
            var eyeDirection = _intention.Forward;
            var delta = GameObject.transform.position.ToVec() - eyePosition;
            var directionToButton = delta.Normalized;

            var eyeDistance = delta.Magnitude;
            var radius = _renderer.Radius;

            var maxTheta = Mathf.Atan2(radius, eyeDistance);

            var cosTheta = Vec3.Dot(
                directionToButton,
                eyeDirection);
            var theta = Mathf.Approximately(cosTheta, 1.0f)
                ? 0.0f
                : Mathf.Acos(cosTheta);

            Aim = Mathf.Approximately(maxTheta, 0.0f)
                ? 0.0f
                : 1.0f - Mathf.Clamp01(Mathf.Abs(theta / maxTheta));
        }

        /// <summary>
        /// Updates the steadiness feedback
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateStability(float deltaTime)
        {
            var targetStability = Focused
                ? _intention.Stability
                : 0.0f;

            const float STABILITY_LERP_RATE_MAGIC_NUMBER = 8.0f;
            var lerp = deltaTime * STABILITY_LERP_RATE_MAGIC_NUMBER;
            Stability = Mathf.Lerp(
                Stability,
                targetStability,
                lerp);
        }

        /// <summary>
        /// Draws debug lines.
        /// </summary>
        /// [Conditional("ELEMENT_DEBUGGING")]
        private void DebugDraw()
        {
            var handle = Render.Handle("IUX.Activator");
            if (null == handle)
            {
                return;
            }

            var pos = _renderer.transform.position;
            var rad = _renderer.Radius;
            handle.Draw(ctx =>
            {
                ctx.Prism(new Bounds(
                    pos,
                    rad * Vector3.one));
            });
        }

        /// <summary>
        /// Sets the icon.
        /// </summary>
        private void SetIcon()
        {
            if (null != _renderer && null != _renderer.Icon)
            {
                _renderer.Icon.sprite = _icon;
                _renderer.Icon.enabled = null != _icon;
            }
        }
    }
}