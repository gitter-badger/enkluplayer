using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class AimableWidget : InteractiveWidget
    {
        /// <summary>
        /// Dependencies
        /// </summary>
        public IIntentionManager Intention { get; private set; }

        /// <summary>
        /// Current aim percentage.
        /// </summary>
        private float _aim;

        /// <summary>
        /// Reflection of intention stability
        /// </summary>
        private float _stability;

        /// <summary>
        /// True if aim is enabled
        /// </summary>
        private bool _isAimEnabled = true;

        /// <summary>
        /// True if aim is enabled
        /// </summary>
        public bool IsAimEnabled
        {
            get { return _isAimEnabled; }
            set { _isAimEnabled = value; }
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
        /// Constructor.
        /// </summary>
        public void Initialize(
            IWidgetConfig config,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors,
            IPrimitiveFactory primitives,
            IMessageRouter messages,
            IIntentionManager intention,
            IInteractionManager interactions)
        {
            Intention = intention;
            Initialize(config, layers, tweens, colors, primitives, messages, interactions);
        }

        /// <summary>
        /// Updates the aim of the activation.
        /// </summary>
        protected override void UpdateInternal()
        {
            base.UpdateInternal();

            if (!IsFocused
             || !IsInteractable
             || !IsAimEnabled)
            {
                _aim = 0.0f;
                _stability = 0.0f;
            }
            else
            {
                UpdateAim();
                UpdateStability(Time.smoothDeltaTime);
            }
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
                = InteractivePrimitive.GetBoundingRadius();

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
                = IsFocused
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
