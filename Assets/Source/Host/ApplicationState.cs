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
        /// Current, parsed application state.
        /// </summary>
        private JObject _state = JObject.Parse("{}");

        /// <inheritdoc cref="IApplicationState"/>
        public bool Get(string path, out string value)
        {
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
        public void Update(string state)
        {
            Log.Info(this, "Received state update.");

            _state = JObject.Parse(state);
        }
    }
}