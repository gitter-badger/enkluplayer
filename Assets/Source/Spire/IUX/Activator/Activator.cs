using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.UI;
using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{ 
    /// <summary>
    /// (IUX Patent)
    /// Logic for activator.
    /// </summary>
    public class Activator : Widget, IActivator
    {
        /// <summary>
        /// Handles raycasts.
        /// </summary>
        private readonly IRaycaster _raycaster;

        /// <summary>
        /// Dependencies.
        /// </summary>
        public IIntentionManager Intention { get; private set; }
        public IInteractionManager Interaction { get; set; }

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<bool> _propHighlighted;
        private ElementSchemaProp<bool> _propInteractionEnabled;
        private ElementSchemaProp<int> _propHighlightPriority;

        /// <summary>
        /// State management for the button
        /// </summary>
        private FiniteStateMachine _states;

        /// <summary>
        /// True if the widget is currently focused
        /// </summary>
        private bool _focused;
        
        /// <summary>
        /// True if aim is enabled
        /// </summary>
        public bool AimEnabled { get; set; }

        /// <summary>
        /// Highligted Accessor/Mutator
        /// </summary>
        public bool Highlighted
        {
            get { return _propHighlighted.Value; }
            set { _propHighlighted.Value = value; }
        }

        /// <summary>
        /// If true, can be interacted with.
        /// </summary>
        public bool InteractionEnabled
        {
            get { return _propInteractionEnabled.Value; }
            set { _propInteractionEnabled.Value = value; }
        }

        /// <summary>
        /// If true, can be interacted with.
        /// </summary>
        public int HighlightPriority
        {
            get { return _propHighlightPriority.Value; }
            set { _propHighlightPriority.Value = value; }
        }

        /// <summary>
        /// Activation percentage
        /// </summary>
        public float Activation { get; set; }

        /// <summary>
        /// Aim percentage.
        /// </summary>
        public float Aim { get; set; }

        /// <summary>
        /// Aim percentage.
        /// </summary>
        public float Stability { get; set; }

        /// <summary>
        /// Bounding radius of the activator.
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// Returns true if interactable.
        /// </summary>
        public bool Interactable
        {
            get
            {
                const float FOCUSABLE_THRESHOLD = 0.99f;
                return Visible && Tween > FOCUSABLE_THRESHOLD
                       && InteractionEnabled
                       && (!Interaction.IsOnRails || Highlighted);
            }
        }

        /// <summary>
        /// True if the widget is focused.
        /// </summary>
        public virtual bool Focused
        {
            get
            {
                return _focused;
            }
            set
            {
                if (_focused != value)
                {
                    _focused = value;

                    if (_focused)
                    {
                        Messages.Publish(MessageTypes.WIDGET_FOCUS, new WidgetFocusEvent());
                    }
                    else
                    {
                        Messages.Publish(MessageTypes.WIDGET_UNFOCUS, new WidgetUnfocusEvent());
                    }
                }
            }
        }

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
        /// Invoked when the widget is activated
        /// </summary>
        public event Action<IActivator> OnActivated = delegate { };

        /// <summary>
        /// Dependency initialization.
        /// </summary>
        public Activator(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IMessageRouter messages,
            IIntentionManager intention, 
            IInteractionManager interaction,
            IRaycaster raycaster,
            float radius)
            : base(gameObject)
        {
            _raycaster = raycaster;

            AimEnabled = true;
            Initialize(config, layers, tweens, colors, messages);

            Intention = intention;
            Interaction = interaction;

            Radius = radius;
        }

        /// <summary>
        /// String representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Activator[{0}]", GameObject.name);
        }

        /// <inheritdoc cref="Element"/>
        protected override void LoadInternal()
        {
            base.LoadInternal();
            
            // States
            {
                _states = new FiniteStateMachine(new IState[]
                {
                    new ActivatorReadyState(this, Schema),
                    new ActivatorActivatingState(this, Schema),
                    new ActivatorActivatedState(this, Schema)
                });
                _states.Change<ActivatorReadyState>();
            }

            // Interaction
            {
                _propHighlighted = Schema.Get<bool>("highlighted");
                _propInteractionEnabled = Schema.Get<bool>("interactionEnabled");
                _propHighlightPriority = Schema.Get<int>("highlightPriority");

                // TODO: fix this once "hasProp" is implemented
                _propInteractionEnabled.Value = true;
            }
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
        /// Changes the state of the activator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ChangeState<T>() where T : ActivatorState
        {
            _states.Change<T>();
        }

        /// <inheritdoc cref="IRaycaster"/>
        public bool Raycast(Vec3 origin, Vec3 direction)
        {
            return _raycaster.Raycast(origin, direction);
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
        /// Updates the aim as a function of focus towards the center of the widget.
        /// </summary>
        private void UpdateAim()
        {
            var eyePosition = Intention.Origin;
            var eyeDirection = Intention.Forward;
            var delta = GameObject.transform.position.ToVec() - eyePosition;
            var directionToButton = delta.Normalized;

            var eyeDistance = delta.Magnitude;
            var radius = Radius;

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
                ? Intention.Stability
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
