using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ButtonActivatedState : IState
    {
        /// <summary>
        /// Affected button.
        /// </summary>
        private readonly Button _button;

        /// <summary>
        /// Called when this state requests a transition.
        /// </summary>
        public event Action<Type> OnTransition;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="button"></param>
        public ButtonActivatedState(Button button)
        {
            _button = button;
        }

        /// <summary>
        /// Invoked when the state is entered.
        /// </summary>
        /// <param name="context"></param>
        public void Enter(object context)
        {
            _button.IsAimEnabled = false;

            if (_button != null
             && _button.Activator != null)
            {
                _button.Activator.Activate();
            }

            var buttonActivateMessage = new ButtonActivateEvent()
            {
                // TODO: Add Data
            };

            _button.Messages.Publish(MessageTypes.BUTTON_ACTIVATE, buttonActivateMessage);
        }

        /// <summary>
        /// Invoked when the state is updated.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (!_button.IsFocused)
            {
                OnTransition(typeof(ButtonReadyState));
            }
        }

        /// <summary>
        /// Invoked when the state is exited.
        /// </summary>
        public void Exit()
        {
            _button.IsAimEnabled = true;
            _button.Activation = 0.0f;
        }
    }
}