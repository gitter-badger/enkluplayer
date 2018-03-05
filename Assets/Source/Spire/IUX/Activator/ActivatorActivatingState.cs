using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ActivatorActivatingState : ActivatorState
    {
        /// <summary>
        /// Configuration for widgets.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// Activator.
        /// </summary>
        private readonly ActivatorPrimitive _activator;

        /// <summary>
        /// Manages intention.
        /// </summary>
        private readonly IIntentionManager _intention;

        /// <summary>
        /// If true, calls activate.
        /// </summary>
        private readonly bool _autoActivate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">Config for widgets.</param>
        /// <param name="activator">Activator.</param>
        /// <param name="intention">Manages intention.</param>
        /// <param name="schema">Schema to use.</param>
        /// <param name="autoActivate">If true, calls Activate.</param>
        public ActivatorActivatingState(
            WidgetConfig config,
            ActivatorPrimitive activator,
            IIntentionManager intention,
            ElementSchema schema,
            bool autoActivate)
            : base(
                schema.Get<string>("activating.color"),
                schema.Get<string>("activating.tween"),
                schema.Get<float>("activating.frameScale"))
        {
            _config = config;
            _activator = activator;
            _intention = intention;
            _autoActivate = autoActivate;
        }

        /// <summary>
        /// Invoked every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            if (!_activator.Focused)
            {
                _activator.Ready();
                return;
            }

            // aim affects fill rate.
            var aim = _activator.Aim;
            var stability = _intention.Stability;
            var fillDuration = _config.GetFillDuration();
            var aimMultiplier = _config.GetFillRateMultiplierFromAim(aim);
            var stabilityMultiplier = _config.GetFillRateMultiplierFromStability(stability);
            var fillRate = aimMultiplier * stabilityMultiplier / fillDuration;
            var deltaFill = deltaTime * fillRate;

            var activation = _activator.Activation + deltaFill;
            _activator.Activation = Mathf.Clamp01(activation);

            if (_autoActivate && (activation > 1.0f || Mathf.Approximately(activation, 1.0f)))
            {
                _activator.Activate();
            }
        }
    }
}