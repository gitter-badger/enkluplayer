namespace CreateAR.SpirePlayer
{
    public interface IInputManager
    {
        void ChangeState(IInputState state);
        void Update(float dt);
    }
}