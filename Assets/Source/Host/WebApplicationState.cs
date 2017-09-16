using CreateAR.Spire;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IApplicationState</c> implementation for the web.
    /// </summary>
    public class WebApplicationState : IApplicationState
    {
        /// <summary>
        /// Helps with communication with webpage.
        /// </summary>
        private readonly WebBridge _bridge;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bridge">The WebBridge.</param>
        public WebApplicationState(WebBridge bridge)
        {
            _bridge = bridge;
        }

        /// <inheritdoc cref="IApplicationState"/>
        public bool Get(string path, out string value)
        {
            value = (string) _bridge.Search(path);
            
            return !string.IsNullOrEmpty(value);
        }
    }
}