using CreateAR.Commons.Unity.Logging;
using Newtonsoft.Json.Linq;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state within the editor.
    /// </summary>
    public class ApplicationState : IApplicationState
    {
        /// <summary>
        /// Message router.
        /// </summary>
        private readonly IMessageRouter _router;

        /// <summary>
        /// Current, parsed application state.
        /// </summary>
        private JObject _state = JObject.Parse("{}");

        /// <summary>
        /// Creates obj.
        /// </summary>
        /// <param name="router">Routes messages.</param>
        public ApplicationState(IMessageRouter router)
        {
            _router = router;
            _router.Subscribe(MessageTypes.STATE, val => Update((string) val));
        }

        /// <inheritdoc cref="IApplicationState"/>
        public bool Get(string path, out string value)
        {
            Log.Debug(this, "Get({0})", path);

            var jval = _state.SelectToken(path);
            if (null == jval)
            {
                value = string.Empty;
                return false;
            }

            value = jval.ToString();
            return true;
        }

        /// <summary>
        /// Receives updates.
        /// </summary>
        /// <param name="state"></param>
        private void Update(string state)
        {
            Log.Info(this, "Received state update.");

            _state = JObject.Parse(state);
        }
    }
}