using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.AR;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Determines application flow for a guest.
    /// </summary>
    public class MobileGuestStateFlow : IStateFlow
    {
        /// <summary>
        /// Ar service.
        /// </summary>
        private readonly IArService _ar;
        
        /// <summary>
        /// Manages flows and states.
        /// </summary>
        private IApplicationStateManager _states;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MobileGuestStateFlow(IArService ar)
        {
            _ar = ar;
        }
        
        /// <inheritdoc />
        public void Start(IApplicationStateManager states)
        {
            _states = states;
            _states.ListenForFlowMessages(
                MessageTypes.VERSION_MISMATCH,
                MessageTypes.LOGIN,
                MessageTypes.USER_PROFILE,
                MessageTypes.LOAD_APP,
                MessageTypes.PLAY,
                MessageTypes.AR_SETUP,
                MessageTypes.ARSERVICE_EXCEPTION,
                MessageTypes.FLOOR_FOUND);
            _states.ChangeState<GuestApplicationState>();
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
                case MessageTypes.VERSION_MISMATCH:
                {
                    _states.ChangeState<VersionMismatchApplicationState>();
                    break;
                }
                case MessageTypes.LOGIN:
                {
                    _states.ChangeFlow<MobileLoginStateFlow>();
                    break;
                }
                case MessageTypes.USER_PROFILE:
                {
                    // nope! redirect
                    _states.ChangeState<GuestApplicationState>();
                    break;
                }
                case MessageTypes.LOAD_APP:
                {
                    _states.ChangeState<LoadAppApplicationState>();
                    break;
                }
                case MessageTypes.PLAY:
                {
                    if (_ar.IsSetup)
                    {
                        _states.ChangeState<PlayApplicationState>();
                    }
                    else
                    {
                        _states.ChangeState<MobileArSetupApplicationState>();
                    }
                    
                    break;
                }
                case MessageTypes.AR_SETUP:
                {
                    _states.ChangeState<MobileArSetupApplicationState>();
                    break;
                }
                case MessageTypes.ARSERVICE_EXCEPTION:
                {
                    _states.ChangeState<MobileArSetupApplicationState>(message);
                    break;
                }
                case MessageTypes.FLOOR_FOUND:
                {
                    _states.ChangeState<PlayApplicationState>();
                    break;
                }
                default:
                {
                    Log.Error(this, "Unhandled MessageType : {0}.", messageType);
                    break;
                }
            }
        }
    }
}