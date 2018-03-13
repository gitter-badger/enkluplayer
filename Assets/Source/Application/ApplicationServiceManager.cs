using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// <c>IApplicationServiceManager</c> implementation.
    /// </summary>
    public class ApplicationServiceManager : IApplicationServiceManager
    {
        /// <summary>
        /// Message dispatch system.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// The bridge into the web world.
        /// </summary>
        private readonly IBridge _bridge;

        /// <summary>
        /// Manages txns.
        /// </summary>
        private readonly IElementTxnManager _txns;
        
        /// <summary>
        /// Filters messages.
        /// </summary>
        private readonly MessageFilter _filter;

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
        /// <param name="messages">Pub/sub.</param>
        /// <param name="bridge">The WebBridge.</param>
        /// <param name="txns">Manages txns.</param>
        /// <param name="filter">Filters messages.</param>
        /// <param name="handler">The object that handles messages.</param>
        /// <param name="services">Services to monitor host.</param>
        public ApplicationServiceManager(
            IMessageRouter messages,
            IBridge bridge,
            IElementTxnManager txns,
            MessageFilter filter,
            BridgeMessageHandler handler,
            ApplicationService[] services)
        {
            _messages = messages;
            _bridge = bridge;
            _txns = txns;
            _filter = filter;
            _handler = handler;
            _services = services;
        }

        /// <inheritdoc cref="IApplicationServiceManager"/>
        public void Start()
        {
            _bridge.Initialize(_handler);

            for (int i = 0, len = _services.Length; i < len; i++)
            {
                _services[i].Start();
            }
            
            // add filter after application is ready
            _messages.Subscribe(
                MessageTypes.APPLICATION_INITIALIZED,
                _ =>
                {
                    // add filters
                    _filter.Filter(new ElementUpdateExclusionFilter(_txns));
                    
                    // ready for action
                    _bridge.BroadcastReady();
                });
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