namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ActivatorActivatedState : ActivatorState
    {
        /// <summary>
        /// Invoked when the state is entered.
        /// </summary>
        /// <param name="context"></param>
        public override void Enter(object context)
        {
            Activator.AimEnabled = false;
            Activator.Messages.Publish(
                MessageTypes.BUTTON_ACTIVATE,
                new ButtonActivateEvent());
        }

        /// <summary>
        /// Invoked when the state is updated.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!Activator.Focused)
            {
                Activator.ChangeState<ActivatorReadyState>();
            }
        }

        /// <summary>
        /// Invoked when the state is exited.
        /// </summary>
        public override void Exit()
        {
            Activator.AimEnabled = true;
            Activator.Activation = 0.0f;
        }
    }
}