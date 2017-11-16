using System;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// State the button takes when it is ready for activation 
    /// but not currently activating.
    /// </summary>
    public class ButtonReadyState : ButtonState
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
        /// Invoked when the state is begun.
        /// </summary>
        /// <param name="context"></param>
        public override void Enter(object context)
        {
            _elapsed = 0.0f;
            _initialActivation = Button.Activation;
        }

        /// <summary>
        /// Invoked every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            if (Button.IsFocused)
            {
                Button.ChangeState<ButtonActivatingState>();
            }
            else
            {
                _elapsed += deltaTime;

                // recede the activation percentage over time
                Button.Activation
                    = _initialActivation
                      * Button
                          .Config
                          .GetFillDelay(_elapsed);
            }
        }
    }
}