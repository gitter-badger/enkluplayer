using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ActivatorActivatingState : ActivatorState
    {
        /// <summary>
        /// Activator.
        /// </summary>
        private readonly ActivatorPrimitive _activator;

        /// <summary>
        /// Manages intention.
        /// </summary>
        private readonly IIntentionManager _intention;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activator">Activator.</param>
        /// <param name="intention">Manages intention.</param>
        /// <param name="schema">Schema to use.</param>
        public ActivatorActivatingState(
            ActivatorPrimitive activator,
            IIntentionManager intention,
            ElementSchema schema)
            : base(
                schema.Get<int>("activating.frameColor"),
                schema.Get<int>("activating.tween"),
                schema.Get<float>("activating.frameScale"))
        {
            _activator = activator;
            _intention = intention;
        }

        /// <summary>
        /// Invoked every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            if (!_activator.Focused)
            {
                _activator.ChangeState<ActivatorReadyState>();
                return;
            }

            // aim affects fill rate.
            var aim = _activator.Aim;
            var stability = _intention.Stability;
            var fillDuration = _activator.Config.GetFillDuration();
            var aimMultiplier = _activator.Config.GetFillRateMultiplierFromAim(aim);
            var stabilityMultiplier = _activator.Config.GetFillRateMultiplierFromStability(stability);
            var fillRate = aimMultiplier * stabilityMultiplier / fillDuration;
            var deltaFill = deltaTime * fillRate;

            var activation = _activator.Activation + deltaFill;
            if (activation > 1.0f || Mathf.Approximately(activation, 1.0f))
            {
                _activator.Activate();
            }
            else
            {
                _activator.Activation = activation;
            }
        }
    }
}