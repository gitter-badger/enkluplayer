using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages major states of the application.
    /// </summary>
    public class ApplicationStateService : ApplicationService
    {
        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Controls application states.
        /// </summary>
        private readonly FiniteStateMachine _states;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ApplicationStateService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            ApplicationConfig config,

            InitializeApplicationState initialize,
            LoginApplicationState login,
            OrientationApplicationState orientation,
            ArSetupApplicationState ar,
            UserProfileApplicationState userProfile,
            LoadAppApplicationState load,
            ReceiveAppApplicationState receive,
            PlayApplicationState play,
            BleSearchApplicationState ble,
            InstaApplicationState insta,
            // TODO: find a different pattern to do this
#if NETFX_CORE
            MeshCaptureApplicationState meshCapture,
#endif
            ToolModeApplicationState tools)
            : base(binder, messages)
        {
            _config = config;
            _states = new FiniteStateMachine(new IState[]
            {
                initialize,
                login,
                orientation,
                ar,
                userProfile,
                load,
                receive,
                play,
                ble,
                insta,
#if NETFX_CORE
                meshCapture,
#endif
                tools
            });
        }

        /// <inheritdoc cref="ApplicationService"/>
        public override void Start()
        {
            Subscribe<Void>(
                MessageTypes.APPLICATION_INITIALIZED,
                Messages_OnApplicationInitialized);

            Subscribe<Type>(
                MessageTypes.CHANGE_STATE,
                _states.Change);

            Subscribe<Void>(
                MessageTypes.LOAD_APP,
                _ =>
                {
                    Log.Info(this, "Load app requested.");

                    _states.Change<LoadAppApplicationState>();
                });

            Subscribe<Void>(
                MessageTypes.LOGIN,
                _ =>
                {
                    Log.Info(this, "Login requested.");

                    _states.Change<LoginApplicationState>();
                });

            Subscribe<Void>(
                MessageTypes.USER_PROFILE,
                _ =>
                {
                    Log.Info(this, "User profile state.");

                    _states.Change<UserProfileApplicationState>();
                });

            Subscribe<Void>(
                MessageTypes.PLAY,
                _ =>
                {
                    Log.Info(this, "Play requested.");

                    _states.Change<PlayApplicationState>();
                });
            
            Subscribe<Void>(
                MessageTypes.TOOLS,
                _ =>
                {
                    Log.Info(this, "Tools requested.");

                    _states.Change<ToolModeApplicationState>();
                });

            Subscribe<Void>(
                MessageTypes.MESHCAPTURE,
                _ =>
                {
                    Log.Info(this, "Message capture requested.");

#if NETFX_CORE
                    _states.Change<MeshCaptureApplicationState>();
#endif
                });
            
            Subscribe<Void>(
                MessageTypes.AR_SETUP,
                _ =>
                {
                    Log.Info(this, "AR setup requested.");
                    
                    _states.Change<ArSetupApplicationState>();
                });
            
            Subscribe<Exception>(
                MessageTypes.ARSERVICE_EXCEPTION,
                exception =>
                {
                    Log.Error(this, "AR Service exception : {0}.", exception.Message);
                    
                    // head back to AR setup
                    _states.Change<ArSetupApplicationState>(exception);
                });
            
            Subscribe<Void>(
                MessageTypes.FLOOR_FOUND,
                _ =>
                {
                    Log.Info(this, "Floor found.");
                    
                    _states.Change<LoadAppApplicationState>();
                });

            _states.Change<InitializeApplicationState>(_config);
        }

        /// <inheritdoc cref="ApplicationService"/>
        public override void Update(float dt)
        {
            base.Update(dt);

            _states.Update(dt);
        }

        /// <inheritdoc cref="ApplicationService"/>
        public override void Stop()
        {
            base.Stop();

            _states.Change(null);
        }

        /// <summary>
        /// Changes to the state given by the state enums.
        /// </summary>
        /// <param name="state">The state to change to.</param>
        private void ChangeState(ApplicationStateType state)
        {
            switch (state)
            {
                case ApplicationStateType.Tool:
                {
                    _states.Change<ToolModeApplicationState>();
                    break;
                }
                case ApplicationStateType.LoadApp:
                {
                    _states.Change<LoadAppApplicationState>();
                    break;
                }
                case ApplicationStateType.UserProfile:
                {
                    _states.Change<UserProfileApplicationState>();
                    break;
                }
                case ApplicationStateType.ReceiveApp:
                {
                    _states.Change<ReceiveAppApplicationState>();
                    break;
                }
                case ApplicationStateType.Insta:
                {
                    _states.Change<InstaApplicationState>();
                    break;
                }
                case ApplicationStateType.Login:
                {
                    _states.Change<LoginApplicationState>();
                    break;
                }
                case ApplicationStateType.Orientation:
                {
                case ApplicationStateTypes.ArSetup:
                {
                    _states.Change<ArSetupApplicationState>();
                    break;
                }
                case ApplicationStateTypes.Orientation:
                {
                    _states.Change<OrientationApplicationState>();
                    break;
                }
                case ApplicationStateType.None:
                {
                    _states.Change(null);
                    break;
                }
                default:
                {
                    throw new Exception(string.Format("Invalid ApplicationState : {0}.", state));
                }
            }
        }

        /// <summary>
        /// Called when application initialized message is published.
        /// </summary>
        /// <param name="_">Void.</param>
        private void Messages_OnApplicationInitialized(Void _)
        {
            Log.Info(this, "Application initialized.");

            var state = ApplicationStateType.Invalid;
            try
            {
                state = (ApplicationStateType) Enum.Parse(
                    typeof(ApplicationStateType),
                    _config.State);
            }

            if (state == ApplicationStateType.Invalid)
            {
                switch (UnityEngine.Application.platform)
                {
                    case RuntimePlatform.WebGLPlayer:
                    {
                        state = ApplicationStateType.ReceiveApp;
                        break;
                    }
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.Android:
                    {
                        state = ApplicationStateType.Login;
                        break;
                    }
                    case RuntimePlatform.WSAPlayerX86:
                    case RuntimePlatform.WSAPlayerARM:
                    case RuntimePlatform.WSAPlayerX64:
                    {
                        state = ApplicationStateType.Orientation;
                        break;
                    }
                    default:
                    {
                        if (!string.IsNullOrEmpty(_config.Play.AppId))
                        {
                            state = ApplicationStateType.LoadApp;
                        }
                        else if (_config.ParsedPlatform == RuntimePlatform.WebGLPlayer)
                        {
                            state = ApplicationStateType.ReceiveApp;
                        }
                        else
                        {
                            state = ApplicationStateType.Login;
                        }

                        break;
                    }
                }
            }

            ChangeState(state);
        }
    }
}