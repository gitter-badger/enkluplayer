﻿using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
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
            QrLoginApplicationState qrLogin,
            OrientationApplicationState orientation,
            UserProfileApplicationState userProfile,
            InputLoginApplicationState inputLogin,
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
                qrLogin,
                orientation,
                userProfile,
                inputLogin,
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

                    switch (UnityEngine.Application.platform)
                    {
                        case RuntimePlatform.WSAPlayerX86:
                        case RuntimePlatform.WSAPlayerX64:
                        case RuntimePlatform.WSAPlayerARM:
                        {
                            _states.Change<QrLoginApplicationState>();
                            break;
                        }
                        default:
                        {
                            _states.Change<InputLoginApplicationState>();
                            break;
                        }
                    }
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
        private void ChangeState(ApplicationStateTypes state)
        {
            switch (state)
            {
                case ApplicationStateTypes.Tool:
                {
                    _states.Change<ToolModeApplicationState>();
                    break;
                }
                case ApplicationStateTypes.LoadApp:
                {
                    _states.Change<LoadAppApplicationState>();
                    break;
                }
                case ApplicationStateTypes.UserProfile:
                {
                    _states.Change<UserProfileApplicationState>();
                    break;
                }
                case ApplicationStateTypes.ReceiveApp:
                {
                    _states.Change<ReceiveAppApplicationState>();
                    break;
                }
                case ApplicationStateTypes.Insta:
                {
                    _states.Change<InstaApplicationState>();
                    break;
                }
                case ApplicationStateTypes.QrLogin:
                {
                    _states.Change<QrLoginApplicationState>();
                    break;
                }
                case ApplicationStateTypes.InputLogin:
                {
                    _states.Change<InputLoginApplicationState>();
                    break;
                }
                case ApplicationStateTypes.Orientation:
                {
                    _states.Change<OrientationApplicationState>();
                    break;
                }
                case ApplicationStateTypes.None:
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

            var state = ApplicationStateTypes.Invalid;
            try
            {
                state = (ApplicationStateTypes)Enum.Parse(
                    typeof(ApplicationStateTypes),
                    _config.StateOverride);
            }
            catch
            {
                //
            }

            if (state == ApplicationStateTypes.Invalid)
            {
                switch (UnityEngine.Application.platform)
                {
                    case RuntimePlatform.WebGLPlayer:
                    {
                        state = ApplicationStateTypes.ReceiveApp;
                        break;
                    }
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.Android:
                    {
                        state = ApplicationStateTypes.InputLogin;
                        break;
                    }
                    case RuntimePlatform.WSAPlayerX86:
                    case RuntimePlatform.WSAPlayerARM:
                    case RuntimePlatform.WSAPlayerX64:
                    {
                        state = ApplicationStateTypes.Orientation;
                        break;
                    }
                }

                // editor can do what it wants
                if (UnityEngine.Application.isEditor)
                {
                    if (_config.SimulateWebgl)
                    {
                        state = ApplicationStateTypes.ReceiveApp;
                    }
                    else
                    {
                        state = ApplicationStateTypes.LoadApp;
                    }
                }
            }

            ChangeState(state);
        }
    }
}