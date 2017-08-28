namespace CreateAR.SpirePlayer
{
    public class EditApplicationState : IApplicationState
    {
        private readonly IInputManager _input;
        private readonly IInputState _defaultState;
        
        public EditApplicationState(
            IInputManager input,
            IInputState defaultState)
        {
            _input = input;
            _defaultState = defaultState;
        }

        public override string ToString()
        {
            return "[EditApplicationState]";
        }

        public void Enter()
        {
            _input.ChangeState(_defaultState);
        }

        public void Update(float dt)
        {
            _input.Update(dt);
        }

        public void Exit()
        {
            
        }
    }
}
