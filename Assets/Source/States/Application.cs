using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class Application
    {
        private IApplicationState _state;

        [Inject("Default")]
        public IApplicationState DefaultState { get; set; }

        public Application()
        {
            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter()));
            Log.AddLogTarget(new FileLogTarget(new DefaultLogFormatter(), "Application.log"));

            Log.Info(this, "Application created.");
        }

        public void ChangeState(IApplicationState state)
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