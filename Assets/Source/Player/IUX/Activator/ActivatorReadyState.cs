namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// State the button takes when it is ready for activation 
    /// but not going to be interacted with.
    /// </summary>
    public class ActivatorReadyState : ActivatorState
    {
        /// <summary>
        /// Activator.
        /// </summary>
        private readonly ActivatorPrimitive _activator;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activator">Activator.</param>
        /// <param name="schema">Schema to use.</param>
        public ActivatorReadyState(
            ActivatorPrimitive activator,
            ElementSchema schema)
            : base(
                schema.Get<string>("ready.color"),
                schema.Get<string>("ready.color"),
                schema.Get<float>("ready.frameScale"))
        {
            _activator = activator;
        }

        /// <summary>
        /// Invoked when the state is begun.
        /// </summary>
        /// <param name="context"></param>
        public override void Enter(object context)
        {
            _activator.Activation = 0;
        }

        /// <summary>
        /// Invoked every frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            if (_activator.Focused)
            {
                _activator.Imminent();
            }
        }
    }
}