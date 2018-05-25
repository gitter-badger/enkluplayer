using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.AR;
using CreateAR.SpirePlayer.States.HoloLogin;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages application flow for a logged in user on mobile.
    /// </summary>
    public class MobileLoginStateFlow : IStateFlow
    {
        /// <summary>
        /// Application wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;
        
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
        public MobileLoginStateFlow(
            ApplicationConfig config,
            IArService ar)
        {
            _config = config;
            _ar = ar;
        }
        
        /// <inheritdoc />
        public void Start(IApplicationStateManager states)
        {
            _states = states;
            _states.ListenForFlowMessages(
                MessageTypes.LOGIN_COMPLETE,
                MessageTypes.USER_PROFILE,
                MessageTypes.LOAD_APP,
                MessageTypes.PLAY,
                MessageTypes.AR_SETUP,
                MessageTypes.ARSERVICE_EXCEPTION,
                MessageTypes.FLOOR_FOUND);
            _states.ChangeState<LoginApplicationState>();
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
                case MessageTypes.LOGIN_COMPLETE:
                {
                    var credentials = _config.Network.Credentials;
                    if (credentials.IsGuest)
                    {
                        _states.ChangeFlow<MobileGuestStateFlow>();
                    }
                    else
                    {
                        _states.ChangeState<LoadDefaultAppApplicationState>();
                    }
                    
                    break;
                }
                case MessageTypes.USER_PROFILE:
                {
                    _states.ChangeState<UserProfileApplicationState>();
                    break;
                }
                case MessageTypes.LOAD_APP:
                {
                    _states.ChangeState<LoadAppApplicationState>();
                    break;
                }
                case MessageTypes.PLAY:
                {
                    // check ar first
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
                case MessageTypes.HOLOLOGIN:
                {
                    _states.ChangeState<HoloLoginApplicationState>();
                    break;
                }
                case MessageTypes.SIGNOUT:
                {
                    _states.ChangeState<SignOutApplicationState>();
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