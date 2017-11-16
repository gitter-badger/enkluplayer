using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ActivatorActivatingState : ActivatorState
    {
        /// <summary>
        /// Invoked when the state is begun.
        /// </summary>
        /// <param name="context"></param>
        public override void Enter(object context)
        {
            if (Activator != null)
            {
                Activator.FillImageVisible = true;
            }
        }

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
            var fillRate
                = Activator.Config.GetFillRateMultiplierFromAim(aim)
                * Activator.Config.GetFillRateMultiplierFromStability(stability)
                / fillDuration;
            var deltaFill
                = deltaTime
                    * fillRate;

            var activation = Activator.Activation + deltaFill;
            if (activation > 1.0f
             || Mathf.Approximately(activation, 1.0f))
            {
                Activator.Activation = 0;
                Activator.ChangeState <ActivatorActivatedState>();
            }
            else
            {
                Activator.Activation = activation;
            }
        }

        /// <summary>
        /// Invoked upon exit
        /// </summary>
        public override void Exit()
        {
            if (Activator.Activator != null)
            {
                Activator.Activator.FillImageVisible = false;
            }
        }
    }
}