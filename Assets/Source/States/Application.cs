using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Root application.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// Current state of the application.
        /// </summary>
        private IApplicationState _state;
        
        /// <summary>
        /// Creates a new Application.
        /// </summary>
        public Application(EditApplicationState defaultState)
        {
            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
            {
                Level = false,
                Timestamp = false
            }));
            Log.AddLogTarget(new FileLogTarget(new DefaultLogFormatter(), "Application.log"));
            Log.Filter = LogLevel.Debug;
            
            ChangeState(defaultState);
        }

        /// <summary>
        /// Changes the application state.
        /// </summary>
        /// <param name="state">Name of the state.</param>
        public void ChangeState(IApplicationState state)
        {
            Log.Debug(this, "Change state from {0} to {1}.",
                null == _state ? "[Null]" : _state.ToString(),
                null == state ? "[Null]" : state.ToString());

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

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">The time since last time Update was called.</param>
        public void Update(float dt)
        {
            if (null != _state)
            {
                _state.Update(dt);
            }
        }
    }
}