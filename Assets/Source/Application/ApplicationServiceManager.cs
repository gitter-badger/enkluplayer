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
        /// Services to monitor host.
        /// </summary>
        private readonly ApplicationService[] _services;

        /// <summary>
        /// Creates a new WebApplicationHost.
        /// </summary>
        /// <param name="bridge">The WebBridge.</param>
        /// <param name="connection">Connection to Trellis.</param>
        /// <param name="services">Services to monitor host.</param>
        public ApplicationServiceManager(
            IBridge bridge,
            IConnection connection,
            ApplicationService[] services)
        {
            _bridge = bridge;
            _connection = connection;
            _services = services;
        }

        /// <inheritdoc cref="IApplicationServiceManager"/>
        public void Start()
        {
            _bridge.Initialize();
            _connection.Connect(null);

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