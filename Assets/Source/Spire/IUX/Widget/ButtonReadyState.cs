using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State the button takes when it is ready for activation 
    /// but not currently activating.
    /// </summary>
    public class ButtonReadyState : IState
    {
        /// <summary>
        /// Activation as the state is entered.
        /// </summary>
        private float _initialActivation;

        /// <summary>
        /// Elapsed time in the state.
        /// </summary>
        private float _elapsed;

        /// <summary>
        /// Parent button.
        /// </summary>
        private readonly Button _button;

        /// <summary>
        /// Called when this state requests a transition.
        /// </summary>
        public event Action<Type> OnTransition;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="button"></param>
        public ButtonReadyState(Button button)
        {
            _button = button;
        }

        /// <summary>
        /// Invoked when the state is begun.
        /// </summary>
        /// <param name="context"></param>
        public void Enter(object context)
        {
            _elapsed = 0.0f;
            _initialActivation = _button.Activation;
        }

        /// <summary>
        /// Invoked every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (_button.IsFocused)
            {
                if (OnTransition != null)
                {
                    OnTransition(typeof(ButtonActivatingState));
                }
            }
            else
            {
                _elapsed += deltaTime;

                // recede the activation percentage over time
                _button.Activation
                    = _initialActivation
                      * _button
                          .Config
                          .FillDecay
                          .Evaluate(_elapsed);
            }
        }

        /// <summary>
        /// Invoked upon exit.
        /// </summary>
        public void Exit()
        {
            
        }
    }
}