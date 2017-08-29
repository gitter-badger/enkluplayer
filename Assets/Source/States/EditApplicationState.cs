using CreateAR.Commons.Unity.Logging;

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
            Log.Info(this, "Enter {0}.", _defaultState);

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
