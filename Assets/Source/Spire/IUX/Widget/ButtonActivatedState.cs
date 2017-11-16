namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ButtonActivatedState : ButtonState
    {
        /// <summary>
        /// Invoked when the state is entered.
        /// </summary>
        /// <param name="context"></param>
        public override void Enter(object context)
        {
            Button.IsAimEnabled = false;

            Button.Activator.ShowActivateVFX();

            var buttonActivateMessage = new ButtonActivateEvent()
            {
                // TODO: Add Data
            };

            Button.Messages.Publish(MessageTypes.BUTTON_ACTIVATE, buttonActivateMessage);
        }

        /// <summary>
        /// Invoked when the state is updated.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!Button.IsFocused)
            {
                Button.ChangeState<ButtonReadyState>();
            }
        }

        /// <summary>
        /// Invoked when the state is exited.
        /// </summary>
        public override void Exit()
        {
            Button.IsAimEnabled = true;
            Button.Activation = 0.0f;
        }
    }
}