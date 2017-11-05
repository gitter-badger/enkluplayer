using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ButtonActivatingState : IState
    {
        /// <summary>
        /// Affected button.
        /// </summary>
        private Button _button;

        /// <summary>
        /// Called when this state requests a transition.
        /// </summary>
        public event Action<Type> OnTransition;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="button"></param>
        public ButtonActivatingState(Button button)
        {
            _button = button;
        }

        /// <summary>
        /// Invoked when the state is begun.
        /// </summary>
        /// <param name="context"></param>
        public void Enter(object context)
        {
            
        }

        /// <summary>
        /// Invoked every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (!_button.IsFocused)
            {
                OnTransition(typeof(ButtonReadyState));
                return;
            }

            // aim affects fill rate.
            var aim = _button.Aim;
            var steadiness = _button.Intention.Steadiness;
            var fillDuration = _button.Config.FillDuration;
            var fillRate
                = _button
                        .Config
                        .AimFillMultiplier
                        .Evaluate(aim)
                    * _button
                        .Config
                        .SteadinessFillMultiplier
                        .Evaluate(steadiness)
                    / fillDuration;
            var deltaFill
                = deltaTime
                    * fillRate;

            var activation = _button.Activation + deltaFill;
            if (activation > 1.0f
                || Mathf.Approximately(activation, 1.0f))
            { 
                _button.Activation = 1.0f;

                OnTransition(typeof(ButtonActivatedState));
            }
            else
            {
                _button.Activation = activation;
            }
        }

        /// <summary>
        /// Invoked upon exit
        /// </summary>
        public void Exit()
        {

        }
    }
}