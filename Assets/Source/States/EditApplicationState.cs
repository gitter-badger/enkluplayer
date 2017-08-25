namespace CreateAR.SpirePlayer
{
    public class EditApplicationState : IApplicationState
    {
        private readonly IMultiInput _input;

        public EditApplicationState(IMultiInput input)
        {
            _input = input;
        }

        public override string ToString()
        {
            return "[EditApplicationState]";
        }

        public void Enter()
        {
            
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
