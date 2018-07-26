using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Determines app flow for Hmd.
    /// </summary>
    public class HmdStateFlow : IStateFlow
    {
        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Manages flows and states.
        /// </summary>
        private IApplicationStateManager _states;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HmdStateFlow(ApplicationConfig config)
        {
            _config = config;
        }
        
        /// <inheritdoc />
        public void Start(IApplicationStateManager states)
        {
            _states = states;
            _states.ListenForFlowMessages(
                MessageTypes.LOGIN,
                MessageTypes.LOGIN_COMPLETE,
                MessageTypes.USER_PROFILE,
                MessageTypes.LOAD_APP,
                MessageTypes.PLAY,
                MessageTypes.ARSERVICE_EXCEPTION,
                MessageTypes.FLOOR_FOUND,
                MessageTypes.MESHCAPTURE,
                MessageTypes.BUGREPORT,
                MessageTypes.DEVICE_REGISTRATION_COMPLETE,
                MessageTypes.SIGNOUT);

            if (UnityEngine.Application.isEditor && _config.IuxDesigner)
            {
                _states.ChangeState<IuxDesignerApplicationState>();
            }
            else
            {
                _states.ChangeState<LoginApplicationState>();
            }
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
                case MessageTypes.LOGIN:
                {
                    _states.ChangeState<LoginApplicationState>();
                    break;
                }
                case MessageTypes.LOGIN_COMPLETE:
                {
                    _states.ChangeState<DeviceRegistrationApplicationState>();
                    break;
                }
                case MessageTypes.DEVICE_REGISTRATION_COMPLETE:
                {
                    _states.ChangeState<LoadDefaultAppApplicationState>();
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
                case MessageTypes.MESHCAPTURE:
                {
                    _states.ChangeState<MeshCaptureApplicationState>();
                    break;
                }
                case MessageTypes.BUGREPORT:
                {
                    _states.ChangeState<BugReportApplicationState>();
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
