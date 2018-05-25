using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Determines app flow for Hmd.
    /// </summary>
    public class HmdStateFlow : IStateFlow
    {
        /// <summary>
        /// Manages flows and states.
        /// </summary>
        private IApplicationStateManager _states;
        
        /// <inheritdoc />
        public void Start(IApplicationStateManager states)
        {
            _states = states;
            _states.ChangeState<OrientationApplicationState>();
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