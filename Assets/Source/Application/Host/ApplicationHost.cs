using Void = CreateAR.Commons.Unity.Async.Void;

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
        /// Services to monitor host.
        /// </summary>
        private readonly ApplicationHostService[] _services;

        /// <summary>
        /// Creates a new WebApplicationHost.
        /// </summary>
        /// <param name="bridge">The WebBridge.</param>
        /// <param name="services">Services to monitor host.</param>
        public ApplicationHost(
            IBridge bridge,
            ApplicationHostService[] services)
        {
            _bridge = bridge;
            _services = services;
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Start()
        {
            _bridge.Initialize();

            foreach (var service in _services)
            {
                service.Start();
            }

            // bind to events from web bridge
            // TODO: Move somewhere else?
            _bridge.Binder.Add<AuthorizedEvent>(MessageTypes.AUTHORIZED);
            _bridge.Binder.Add<PreviewEvent>(MessageTypes.PREVIEW);
            _bridge.Binder.Add<Void>(MessageTypes.HIERARCHY);
            
            // tell the webpage
            _bridge.BroadcastReady();
        }

        /// <inheritdoc cref="IApplicationHost"/>
        public void Stop()
        {
            foreach (var service in _services)
            {
                service.Stop();
            }

            _bridge.Binder.Clear();
            _bridge.Uninitialize();
        }
    }
}