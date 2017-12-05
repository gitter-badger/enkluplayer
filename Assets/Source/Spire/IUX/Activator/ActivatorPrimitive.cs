using System;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.UI;
using UnityEngine;

using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    public class ActivatorPrimitive : Widget, IInteractable
    {
        private readonly WidgetConfig _config;
        private readonly IInteractableManager _interactables;
        private readonly IInteractionManager _interaction;
        private readonly IIntentionManager _intention;
        private readonly IMessageRouter _messages;
        private ActivatorRenderer _renderer;

        private ElementSchemaProp<bool> _propInteractionEnabled;
        private ElementSchemaProp<bool> _propHighlighted;
        private ElementSchemaProp<int> _propHighlightPriority;

        /// <summary>
        /// State management for the button
        /// </summary>
        private FiniteStateMachine _states;

        /// <summary>ra
        /// Backing variable for Focused property.
        /// </summary>
        private bool _focused;

        /// <inheritdoc cref="IInteractable"/>
        public bool Interactable
        {
            get
            {
                const float FOCUSABLE_THRESHOLD = 0.99f;
                return Visible && Tween > FOCUSABLE_THRESHOLD
                       && InteractionEnabled
                       && (!_interaction.IsOnRails || Highlighted);
            }
        }

        /// <inheritdoc cref="IInteractable"/>
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

                if (_focused)
                {
                    _messages.Publish(MessageTypes.WIDGET_FOCUS, new WidgetFocusEvent());
                }
                else
                {
                    _messages.Publish(MessageTypes.WIDGET_UNFOCUS, new WidgetUnfocusEvent());
                }
            }
        }

        /// <inheritdoc cref="IInteractable"/>
        public int HighlightPriority
        {
            get { return _propHighlightPriority.Value; }
            set { _propHighlightPriority.Value = value; }
        }

        /// <inheritdoc cref="IRaycaster"/>
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
        /// True iff the widget should be highlighted.
        /// </summary>
        public bool Highlighted
        {
            get { return _propHighlighted.Value; }
            set { _propHighlighted.Value = value; }
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
                return (ActivatorState)_states.Current;
            }
        }

        /// <summary>
        /// Invoked when the activator is activated.
        /// </summary>
        public event Action<ActivatorPrimitive> OnActivated;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActivatorPrimitive(
            WidgetConfig config,
            IInteractableManager interactables,
            IInteractionManager interaction,
            IIntentionManager intention,
            IMessageRouter messages,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors)
        {
            _config = config;
            _interactables = interactables;
            _interaction = interaction;
            _intention = intention;
            _messages = messages;

            Initialize(config, layers, tweens, colors, messages);
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
        protected override void LoadInternal()
        {
            base.LoadInternal();

            _renderer = Object.Instantiate(
                _config.Activator,
                Vector3.zero,
                Quaternion.identity);
            _renderer.transform.SetParent(GameObject.transform, false);

            AimEnabled = true;

            InitializeProps();
            InitializeStates();

            _interactables.Add(this);
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            _interactables.Remove(this);

            Object.Destroy(_renderer.gameObject);

            base.UnloadInternal();
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
        }

        /// <summary>
        /// Changes the state of the activator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ChangeState<T>() where T : ActivatorState
        {
            _states.Change<T>();
        }

        /// <summary>
        /// Forced activation.
        /// </summary>
        public void Activate()
        {
            _states.Change<ActivatorActivatedState>();

            Activation = 0;

            _renderer.Activate();

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

            var hasProp = Schema.HasProp("interactionEnabled");
            _propInteractionEnabled = Schema.Get<bool>("interactionEnabled");

            // default to true
            if (!hasProp)
            {
                _propInteractionEnabled.Value = true;
            }
        }

        /// <summary>
        /// Initializes sub-states.
        /// </summary>
        private void InitializeStates()
        {
            _states = new FiniteStateMachine(new IState[]
            {
                new ActivatorReadyState(this, Schema),
                new ActivatorActivatingState(this, _intention, Schema),
                new ActivatorActivatedState(this, _messages, Schema)
            });
            _states.Change<ActivatorReadyState>();
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
    }
}