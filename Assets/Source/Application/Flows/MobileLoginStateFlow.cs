using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.AR;
using CreateAR.EnkluPlayer.States.HoloLogin;

namespace CreateAR.EnkluPlayer
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
                MessageTypes.VERSION_MISMATCH,
                MessageTypes.VERSION_UPGRADE,
                MessageTypes.LOGIN,
                MessageTypes.LOGIN_COMPLETE,
                MessageTypes.USER_PROFILE,
                MessageTypes.LOAD_APP,
                MessageTypes.PLAY,
                MessageTypes.AR_SETUP,
                MessageTypes.HOLOLOGIN,
                MessageTypes.ARSERVICE_EXCEPTION,
                MessageTypes.FLOOR_FOUND,
                MessageTypes.SIGNOUT);
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
                case MessageTypes.VERSION_MISMATCH:
                {
                    _states.ChangeState<VersionErrorApplicationState>(new VersionErrorApplicationState.VersionError
                    {
                        Message = "This version of Enklu is no longer supported. Please upgrade to access your experiences."
                    });
                    break;
                }
                case MessageTypes.VERSION_UPGRADE:
                {
                    _states.ChangeState<VersionErrorApplicationState>(new VersionErrorApplicationState.VersionError
                    {
                        Message = "This version of Enklu is old news! An update is available."
                    });
                    break;
                }
                case MessageTypes.LOGIN:
                {
                    _states.ChangeState<LoginApplicationState>();
                    break;
                }
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