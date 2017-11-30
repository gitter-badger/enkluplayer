using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class ActivatorActivatedState : ActivatorState
    {
        /// <summary>
        /// Activator.
        /// </summary>
        private readonly ActivatorMonoBehaviour _activator;

        /// <summary>
        /// Routes messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activator">Activator.</param>
        /// <param name="messages">Routes messages.</param>
        /// <param name="schema">Schema to use.</param>
        public ActivatorActivatedState(
            ActivatorMonoBehaviour activator,
            IMessageRouter messages,
            ElementSchema schema)
            : base(
                schema.Get<int>("activated.frameColor"),
                schema.Get<int>("activated.tween"),
                schema.Get<float>("activated.frameScale"))
        {
            _activator = activator;
            _messages = messages;
        }

        /// <summary>
        /// Invoked when the state is entered.
        /// </summary>
        /// <param name="context"></param>
        public override void Enter(object context)
        {
            _activator.AimEnabled = false;
            _messages.Publish(
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