using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ActivatorActivatingState : ActivatorState
    {
        /// <summary>
        /// Invoked every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            if (!Activator.Focused)
            {
                Activator.ChangeState<ActivatorReadyState>();
                return;
            }

            // aim affects fill rate.
            var aim = Activator.Aim;
            var stability = Activator.Intention.Stability;
            var fillDuration = Activator.Config.GetFillDuration();
            var aimMultiplier = Activator.Config.GetFillRateMultiplierFromAim(aim);
            var stabilityMultiplier = Activator.Config.GetFillRateMultiplierFromStability(stability);
            var fillRate = aimMultiplier * stabilityMultiplier / fillDuration;
            var deltaFill = deltaTime * fillRate;

            var activation = Activator.Activation + deltaFill;
            if (activation > 1.0f || Mathf.Approximately(activation, 1.0f))
            {
                Activator.Activate();
            }
            else
            {
                Activator.Activation = activation;
            }
        }
    }
}