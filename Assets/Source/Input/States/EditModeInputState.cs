namespace CreateAR.SpirePlayer
{
    public class EditModeInputState : IState
    {
        private readonly IMultiInput _input;
        private readonly MainCamera _camera;
        private readonly InputConfig _config;

        private FiniteStateMachine _states;

        public EditModeInputState(
            IMultiInput input,
            MainCamera camera,
            InputConfig config)
        {
            _input = input;
            _camera = camera;
            _config = config;
        }

        public void Enter()
        {
            var idle = new EditIdleState(_input);
            idle.OnNext += type => _states.Change(type);

            var pan = new EditPanState(_input, _camera, _config);
            pan.OnNext += type => _states.Change(type);

            var rotate = new EditRotateState(_input, _camera, _config);
            rotate.OnNext += type => _states.Change(type);

            _states = new FiniteStateMachine(new IState[]
            {
                idle, pan, rotate
            });

            _states.Change<EditIdleState>();
        }

        public void Update(float dt)
        {
            _states.Update(dt);
        }

        public void Exit()
        {
            _states.Change<EditIdleState>();
            _states = null;
        }
    }
}