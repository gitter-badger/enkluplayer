using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ButtonActivatingState : ButtonState
    {
        /// <summary>
        /// Invoked when the state is begun.
        /// </summary>
        /// <param name="context"></param>
        public override void Enter(object context)
        {
            if (Button.Activator != null)
            {
                Button.Activator.FillImageVisible = true;
            }
        }

        /// <summary>
        /// Invoked every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            if (!Button.IsFocused)
            {
                Button.ChangeState<ButtonReadyState>();
                return;
            }

            // aim affects fill rate.
            var aim = Button.Aim;
            var stability = Button.Intention.Stability;
            var fillDuration = Button.Config.GetFillDuration();
            var fillRate
                = Button.Config.GetFillRateMultiplierFromAim(aim)
                * Button.Config.GetFillRateMultiplierFromStability(stability)
                / fillDuration;
            var deltaFill
                = deltaTime
                    * fillRate;

            var activation = Button.Activation + deltaFill;
            if (activation > 1.0f
             || Mathf.Approximately(activation, 1.0f))
            {
                Button.Activation = 0;
                Button.ChangeState <ButtonActivatedState>();
            }
            else
            {
                Button.Activation = activation;
            }
        }

        /// <summary>
        /// Invoked upon exit
        /// </summary>
        public override void Exit()
        {
            if (Button.Activator != null)
            {
                Button.Activator.FillImageVisible = false;
            }
        }
    }
}