using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// State flow management for standalone player.
    /// </summary>
    public class StandaloneStateFlow : IStateFlow
    {
        /// <summary>
        /// Manages state and flow transitions.
        /// </summary>
        private IApplicationStateManager _states;

        /// <inheritdoc />
        public void Start(IApplicationStateManager states)
        {
            Log.Info(this, "Starting standalone flow.");

            _states = states;
            _states.ListenForFlowMessages(
                MessageTypes.VERSION_MISMATCH,
                MessageTypes.VERSION_UPGRADE,
                MessageTypes.LOGIN,
                MessageTypes.LOGIN_COMPLETE,
                MessageTypes.LOAD_APP,
                MessageTypes.PLAY,
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
                        Message = "This version of Enklu is old news! An update is available.",
                        AllowContinue = true
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
                    _states.ChangeState<LoadDefaultAppApplicationState>();
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