using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class Application
    {
        private IApplicationState _state;

        /// <summary>
        /// Default state (named injections are not allowed in constructor).
        /// </summary>
        [Inject("Default")]
        public IApplicationState DefaultState { get; set; }

        /// <summary>
        /// Creates a new Application.
        /// </summary>
        public Application()
        {
            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
            {
                Level = false,
                Timestamp = false
            }));
            Log.AddLogTarget(new FileLogTarget(new DefaultLogFormatter(), "Application.log"));

            Log.Filter = LogLevel.Debug;
            Log.Info(this, "Application created.");
        }

        public void ChangeState(IApplicationState state)
        {
            Log.Debug(this, "Change state from {0} to {1}.",
                _state,
                state);

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
            if (null == _state)
            {
                ChangeState(DefaultState);
            }

            if (null != _state)
            {
                _state.Update(dt);
            }
        }
    }
}