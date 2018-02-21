namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IApplicationServiceManager</c> implementation.
    /// </summary>
    public class ApplicationServiceManager : IApplicationServiceManager
    {
        /// <summary>
        /// The bridge into the web world.
        /// </summary>
        private readonly IBridge _bridge;

        /// <summary>
        /// Connection to Trellis.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Filters messages.
        /// </summary>
        private readonly MessageFilter _filter;

        /// <summary>
        /// Application config.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Services to monitor host.
        /// </summary>
        private readonly ApplicationService[] _services;

        /// <summary>
        /// Handles messages from the bridge.
        /// </summary>
        private readonly BridgeMessageHandler _handler;

        /// <summary>
        /// Creates a new WebApplicationHost.
        /// </summary>
        /// <param name="bridge">The WebBridge.</param>
        /// <param name="connection">Connection to Trellis.</param>
        /// <param name="filter">Filters messages.</param>
        /// <param name="handler">The object that handles messages.</param>
        /// <param name="config">Application wide config.</param>
        /// <param name="services">Services to monitor host.</param>
        public ApplicationServiceManager(
            IBridge bridge,
            IConnection connection,
            MessageFilter filter,
            BridgeMessageHandler handler,
            ApplicationConfig config,
            ApplicationService[] services)
        {
            _bridge = bridge;
            _connection = connection;
            _filter = filter;
            _handler = handler;
            _config = config;
            _services = services;
        }

        /// <inheritdoc cref="IApplicationServiceManager"/>
        public void Start()
        {
            var current = _config.Network.Current;

            // add filters
            _filter.Filter(new ElementUpdateExclusionFilter(_config
                .Network
                .Credentials(current)
                .UserId));

            _bridge.Initialize(_handler);
            _connection.Connect(_config.Network.Environment(current));

            for (int i = 0, len = _services.Length; i < len; i++)
            {
                _services[i].Start();
            }
            
            // tell the webpage
            _bridge.BroadcastReady();
        }

        /// <inheritdoc cref="IApplicationServiceManager"/>
        public void Update(float dt)
        {
            for (int i = 0, len = _services.Length; i < len; i++)
            {
                _services[i].Update(dt);
            }
        }

        /// <inheritdoc cref="IApplicationServiceManager"/>
        public void Stop()
        {
            for (int i = 0, len = _services.Length; i < len; i++)
            {
                _services[i].Stop();
            }

            _bridge.Uninitialize();
        }
    }
}