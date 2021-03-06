﻿using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.AR;

namespace CreateAR.EnkluPlayer
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
                MessageTypes.VERSION_UPGRADE,
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