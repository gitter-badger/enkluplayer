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
        /// Constructor.
        /// </summary>
        /// <param name="messages">Pub/sub.</param>
        /// <param name="txns">Manages txns.</param>
        /// <param name="filter">Filters messages.</param>
        /// <param name="services">Services to monitor host.</param>
        public ApplicationServiceManager(
            IMessageRouter messages,
            IElementTxnManager txns,
            MessageFilter filter,
            ApplicationService[] services)
        {
            _messages = messages;
            _txns = txns;
            _filter = filter;
            _services = services;
        }

        /// <inheritdoc />
        public void Start()
        {
            // add filter after application is ready
            _messages.Subscribe(
                MessageTypes.APPLICATION_INITIALIZED,
                _ =>
                {
                    // add filters
                    _filter.Filter(new ElementUpdateExclusionFilter(_txns));
                });
            
            for (int i = 0, len = _services.Length; i < len; i++)
            {
                _services[i].Start();
            }
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            for (int i = 0, len = _services.Length; i < len; i++)
            {
                _services[i].Update(dt);
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            for (int i = 0, len = _services.Length; i < len; i++)
            {
                _services[i].Stop();
            }
        }

        /// <inheritdoc />
        public void Suspend()
        {
            _messages.Publish(MessageTypes.APPLICATION_SUSPEND);
        }

        /// <inheritdoc />
        public void Resume()
        {
            _messages.Publish(MessageTypes.APPLICATION_RESUME);
        }
    }
}