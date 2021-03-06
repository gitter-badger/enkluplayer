﻿using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
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
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Manages flows and states.
        /// </summary>
        private IApplicationStateManager _states;

        /// <summary>
        /// Timer id for time to play.
        /// </summary>
        private int _timeToPlayTimer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HmdStateFlow(
            ApplicationConfig config,
            IMetricsService metrics)
        {
            _config = config;
            _metrics = metrics;

            _timeToPlayTimer = _metrics.Timer(MetricsKeys.STATE_TIMETOPLAY).Start();
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
                MessageTypes.ARSERVICE_EXCEPTION,
                MessageTypes.FLOOR_FOUND,
                MessageTypes.BUGREPORT,
                MessageTypes.DEVICE_REGISTRATION,
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
                case MessageTypes.DEVICE_REGISTRATION:
                case MessageTypes.LOGIN_COMPLETE:
                {
                    if (_config.Play.SkipDeviceRegistration)
                    {
                        Log.Info(this, "Skipping device registration.");

                        _states.ChangeState<LoadDefaultAppApplicationState>();
                    }
                    else
                    {
                        _states.ChangeState<DeviceRegistrationApplicationState>();
                    }
                    
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
                    _metrics.Timer(MetricsKeys.STATE_TIMETOPLAY).Stop(_timeToPlayTimer);

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
