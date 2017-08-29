namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Input state for edit mode.
    /// </summary>
    public class EditModeInputState : IState
    {
        /// <summary>
        /// Input machanism.
        /// </summary>
        private readonly IMultiInput _input;

        /// <summary>
        /// The scene's main camera.
        /// </summary>
        private readonly MainCamera _camera;

        /// <summary>
        /// Input configuration.
        /// </summary>
        private readonly InputConfig _config;

        /// <summary>
        /// An FSM for substates.
        /// </summary>
        private FiniteStateMachine _states;
        
        /// <summary>
        /// Creates a new input state.
        /// </summary>
        public EditModeInputState(
            IMultiInput input,
            MainCamera camera,
            InputConfig config)
        {
            _input = input;
            _camera = camera;
            _config = config;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter()
        {
            var idle = new EditIdleState(_input);
            idle.OnTransition += type => _states.Change(type);

            var pan = new EditPanState(_input, _camera, _config);
            pan.OnTransition += type => _states.Change(type);

            var rotate = new EditRotateState(_input, _camera, _config);
            rotate.OnTransition += type => _states.Change(type);

            _states = new FiniteStateMachine(new IState[]
            {
                idle, pan, rotate
            });

            _states.Change<EditIdleState>();
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            _states.Update(dt);
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            _states.Change<EditIdleState>();
            _states = null;
        }
    }
}