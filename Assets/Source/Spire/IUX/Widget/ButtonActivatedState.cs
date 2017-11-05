using System;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ButtonActivatedState : IState
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
            InteractableWidget.OnRails = false;
            _button.IsInteractionEnabled = false;

            // TODO: Send button activation messages
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
            _button.Activation = 0.0f;
            _button.IsInteractionEnabled = true;
        }
    }
}