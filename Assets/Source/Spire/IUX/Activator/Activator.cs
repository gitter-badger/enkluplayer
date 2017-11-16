﻿using CreateAR.SpirePlayer;
using CreateAR.SpirePlayer.UI;
using System.Linq;
using UnityEngine;

namespace CreateAR.SpireSplayer
{
    public class Activator : Widget, IActivator
    {
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
        /// Current aim percentage.
        /// </summary>
        private float _aim;

        /// <summary>
        /// True if aim is enabled
        /// </summary>
        private bool _aimEnabled = true;

        /// <summary>
        /// Reflection of intention stability
        /// </summary>
        private float _stability;

        /// <summary>
        /// Reflection of intention duration
        /// </summary>
        private float _activation;

        /// <summary>
        /// True if the widget is currently focused
        /// </summary>
        private bool _focused;

        /// <summary>
        /// True if aim is enabled
        /// </summary>
        public bool AimEnabled
        {
            get { return _aimEnabled; }
            set { _aimEnabled = value; }
        }

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
        public float Activation
        {
            get { return _activation; }
            set { _activation = value; }
        }

        /// <summary>
        /// Aim percentage.
        /// </summary>
        public float Aim
        {
            get { return _aim; }
            set { _aim = value; }
        }

        /// <summary>
        /// Aim percentage.
        /// </summary>
        public float Stability
        {
            get { return _stability; }
            set { _stability = value; }
        }

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
        /// Dependency initialization.
        /// </summary>
        /// <param name="intention"></param>
        /// <param name="interaction"></param>
        public void Initialize(IIntentionManager intention, IInteractionManager interaction)
        {
            Intention = intention;
            Interaction = interaction;
        }

        /// <summary>
        /// Prop Initialization.
        /// </summary>
        protected override void LoadInternal()
        {
            base.LoadInternal();

            // States
            {
                var activatorState
                    = Find("states.*")
                        .Cast<IState>()
                        .ToArray();
                for (int i = 0, count = activatorState.Length; i < count; ++i)
                {
                    var buttonState = activatorState[i] as ActivatorState;
                    if (buttonState != null)
                    {
                        buttonState.Activator = this;
                    }
                }

                _states = new FiniteStateMachine(activatorState);
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
        /// Changes the state of the activator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ChangeState<T>() where T : ActivatorState
        {
            _states.Change<T>();
        }

        /// <summary>
        /// Called once every frame.
        /// </summary>
        protected override void UpdateInternal()
        {
            base.UnloadInternal();

            var deltaTime = Time.smoothDeltaTime;

            if (!Focused
             || !Interactable
             || !AimEnabled)
            {
                _aim = 0.0f;
                _stability = 0.0f;
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
            var eyePosition
                = Intention
                    .Origin;
            var eyeDirection
                = Intention
                    .Forward;
            var delta
                = GameObject
                      .transform
                      .position.ToVec() - eyePosition;
            var directionToButton
                = delta
                    .Normalized;

            var eyeDistance
                = delta
                    .Magnitude;
            var radius
                = GetBoundingRadius();

            var maxTheta
                = Mathf.Atan2(radius, eyeDistance);

            var cosTheta
                = Vec3
                    .Dot(
                        directionToButton,
                        eyeDirection);
            var theta
                = Mathf.Approximately(cosTheta, 1.0f)
                    ? 0.0f
                    : Mathf.Acos(cosTheta);

            _aim
                = Mathf.Approximately(maxTheta, 0.0f)
                    ? 0.0f
                    : 1.0f
                      - Mathf
                          .Clamp01(
                              Mathf
                                  .Abs(
                                      theta / maxTheta));
        }

        /// <summary>
        /// Updates the steadiness feedback
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateStability(float deltaTime)
        {
            var targetStability
                = Focused
                    ? Intention.Stability
                    : 0.0f;

            const float STABILITY_LERP_RATE_MAGIC_NUMBER = 8.0f;
            var lerp = deltaTime * STABILITY_LERP_RATE_MAGIC_NUMBER;
            _stability
                = Mathf.Lerp(
                    _stability,
                    targetStability,
                    lerp);
        }
    }
}
