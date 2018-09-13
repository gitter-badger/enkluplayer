using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.AR;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Flow for the web!
    /// </summary>
    public class WebStateFlow : IStateFlow
    {
        /// <summary>
        /// Ar service.
        /// </summary>
        private readonly IArService _ar;
        
        /// <summary>
        /// Configuration for Ar service.
        /// </summary>
        private readonly ArServiceConfiguration _config;
        
        /// <summary>
        /// Manages states and flows.
        /// </summary>
        private IApplicationStateManager _states;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WebStateFlow(
            IArService ar,
            ArServiceConfiguration config)
        {
            _ar = ar;
            _config = config;
        }
        
        /// <inheritdoc />
        public void Start(IApplicationStateManager states)
        {
            _states = states;
            _states.ListenForFlowMessages(MessageTypes.PLAY);
            
            _ar.Setup(_config);
            _states.ChangeState<ReceiveAppApplicationState>();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _states = null;
        }

        /// <inheritdoc />
        public void MessageReceived(int messageType, object message)
        {
            switch (messageType)
            {
                case MessageTypes.PLAY:
                {
                    _states.ChangeState<PlayApplicationState>();
                    break;
                }
                default:
                {
                    Log.Error(this, "Unhandled message type : {0}.", messageType);
                    break;
                }
            }
        }
    }
}