namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// State the button takes when the user might activate it soon.
    /// </summary>
    public class ActivatorImminentState : ActivatorState
    {
        /// <summary>
        /// Configuration for widgets.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// Activator.
        /// </summary>
        private readonly ActivatorPrimitive _activator;

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
        /// <param name="config">Configuration for widgets.</param>
        /// <param name="activator">Activator.</param>
        /// <param name="schema">Schema to use.</param>
        public ActivatorImminentState(
            WidgetConfig config,
            ActivatorPrimitive activator,
            ElementSchema schema)
            : base(
                schema.Get<string>("ready.color"),
                schema.Get<string>("ready.color"),
                schema.Get<float>("ready.frameScale"))
        {
            _config = config;
            _activator = activator;
        }

        /// <inheritdoc />
        public override void Enter(object context)
        {
            _elapsed = 0.0f;
            _initialActivation = _activator.Activation;
        }

        /// <inheritdoc />
        public override void Update(float deltaTime)
        {
            if (!_activator.Focused)
            {
                _activator.Ready();
            }
            else if (_activator.Aim >= 0)
            {
                _activator.Activating();
            }
            else
            {
                _elapsed += deltaTime;

                // recede the activation percentage over time
                _activator.Activation = _initialActivation * _config.GetFillDelay(_elapsed);
            }
        }
    }
}