using CreateAR.Commons.Unity.Logging;
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
        /// Manages flows and states.
        /// </summary>
        private IApplicationStateManager _states;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MobileLoginStateFlow(ApplicationConfig config)
        {
            _config = config;
        }
        
        /// <inheritdoc />
        public void Start(IApplicationStateManager states)
        {
            _states = states;
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
                    _states.ChangeState<PlayApplicationState>();
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