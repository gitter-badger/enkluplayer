namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// State the button takes when it is ready for activation 
    /// but not currently activating.
    /// </summary>
    public class ActivatorReadyState : ActivatorState
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
        /// Constructor.
        /// </summary>
        /// <param name="activator">Activator.</param>
        /// <param name="schema">Schema to use.</param>
        public ActivatorReadyState(
            Activator activator,
            ElementSchema schema)
            : base(
                activator,
                schema.Get<int>("ready.frameColor"),
                schema.Get<int>("ready.tween"),
                schema.Get<float>("ready.frameScale"))
        {
            //
        }

        /// <summary>
        /// Invoked when the state is begun.
        /// </summary>
        /// <param name="context"></param>
        public override void Enter(object context)
        {
            _elapsed = 0.0f;
            _initialActivation = _activator.Activation;
        }

        /// <summary>
        /// Invoked every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            if (_activator.Focused)
            {
                _activator.ChangeState<ActivatorActivatingState>();
            }
            else
            {
                _elapsed += deltaTime;

                // recede the activation percentage over time
                _activator.Activation = _initialActivation * _activator
                    .Config
                    .GetFillDelay(_elapsed);
            }
        }
    }
}