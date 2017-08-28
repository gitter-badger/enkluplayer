namespace CreateAR.SpirePlayer
{
    public class StateMachine
    {
        private IState _state;

        public void Change(IState state)
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

    public interface IState
    {
        void Enter();
        void Update(float dt);
        void Exit();
    }
}
