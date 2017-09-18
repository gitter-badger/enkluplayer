using CreateAR.Commons.Unity.Logging;
using CreateAR.Spire;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IApplicationHost</c> implementation for webpages.
    /// </summary>
    public class ApplicationHost : IApplicationHost
    {
        /// <summary>
        /// The bridge into the web world.
        /// </summary>
        private readonly IBridge _bridge;

        /// <summary>
        /// Creates a new WebApplicationHost.
        /// </summary>
        /// <param name="bridge">The WebBridge.</param>
        public ApplicationHost(IBridge bridge)
        {
            _bridge = bridge;
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Ready()
        {
            // bind to events from web bridge
            _bridge.Bind<AuthorizedEvent>("Authorized", MessageTypes.AUTHORIZED);
            _bridge.Bind<PreviewEvent>("Preview", MessageTypes.PREVIEW);

            // tell the webpage
            _bridge.BroadcastReady();
        }
    }
}