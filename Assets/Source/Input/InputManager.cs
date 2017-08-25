namespace CreateAR.SpirePlayer
{
    public interface IInputState
    {
        void Enter();
        void Update(float dt);
        void Exit();
    }

    public interface IInputManager
    {
        void ChangeState(IInputState state);
        void Update(float dt);
    }

    public class InputManager : IInputManager
    {
        private readonly IMultiInput _input;

        private IInputState _state;

        public InputManager(IMultiInput input)
        {
            _input = input;
        }


        public void ChangeState(IInputState state)
        {
            if (null != _state)
            {
                _state.Exit();
            }

            _state = state;

            if (null != _state)
            {
                _state.Enter();
            }
        }

        public void Update(float dt)
        {
            if (null != _state)
            {
                _state.Update(dt);
            }
        }
    }
}