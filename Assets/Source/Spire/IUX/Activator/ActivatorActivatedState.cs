namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ActivatorActivatedState : ActivatorState
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activator">Activator.</param>
        /// <param name="schema">Schema to use.</param>
        public ActivatorActivatedState(
            Activator activator,
            ElementSchema schema)
            : base(
                activator,
                schema.Get<int>("activated.frameColor"),
                schema.Get<int>("activated.tween"),
                schema.Get<float>("activated.frameScale"))
        {
            //
        }

        /// <summary>
        /// Invoked when the state is entered.
        /// </summary>
        /// <param name="context"></param>
        public override void Enter(object context)
        {
            _activator.AimEnabled = false;
            _activator.Messages.Publish(
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

            if (!_activator.Focused)
            {
                _activator.ChangeState<ActivatorReadyState>();
            }
        }

        /// <summary>
        /// Invoked when the state is exited.
        /// </summary>
        public override void Exit()
        {
            _activator.AimEnabled = true;
            _activator.Activation = 0.0f;
        }
    }
}