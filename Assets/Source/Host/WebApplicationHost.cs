using CreateAR.Commons.Unity.Logging;
using CreateAR.Spire;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IApplicationHost</c> implementation for webpages.
    /// </summary>
    public class WebApplicationHost : IApplicationHost
    {
        /// <summary>
        /// The bridge into the web world.
        /// </summary>
        private readonly WebBridge _bridge;

        /// <summary>
        /// Creates a new WebApplicationHost.
        /// </summary>
        /// <param name="bridge">The WebBridge.</param>
        public WebApplicationHost(WebBridge bridge)
        {
            _bridge = bridge;
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Ready()
        {
            Log.Info(this, "Application is ready.");

            // bind to events from web bridge
            _bridge.Bind<AuthorizedEvent>("Authorized", MessageTypes.AUTHORIZED);

            _bridge.BroadcastReady();
        }
    }
}