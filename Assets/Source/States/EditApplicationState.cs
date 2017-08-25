namespace CreateAR.SpirePlayer
{
    public class EditApplicationState : IApplicationState
    {
        private readonly IInputManager _input;

        [Inject("EditMode")]
        public IInputState InputState { get; set; }

        public EditApplicationState(IInputManager input)
        {
            _input = input;
        }

        public override string ToString()
        {
            return "[EditApplicationState]";
        }

        public void Enter()
        {
            _input.ChangeState(InputState);
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
