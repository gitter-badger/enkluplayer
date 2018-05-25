using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.States.HoloLogin;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages major states of the application.
    /// </summary>
    public class ApplicationStateService : ApplicationService, IApplicationStateManager
    {
        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Controls application states.
        /// </summary>
        private readonly FiniteStateMachine _fsm;

        /// <summary>
        /// Potential flows.
        /// </summary>
        private readonly IStateFlow[] _flows;

        /// <summary>
        /// The current flow.
        /// </summary>
        private IStateFlow _flow;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ApplicationStateService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            ApplicationConfig config,
            IStateFlow[] flows,

            InitializeApplicationState initialize,
            LoginApplicationState login,
            HoloLoginApplicationState holoLogin,
            SignOutApplicationState signOut,
            GuestApplicationState guest,
            OrientationApplicationState orientation,
            MobileArSetupApplicationState mobileAr,
            UserProfileApplicationState userProfile,
            LoadAppApplicationState load,
            LoadDefaultAppApplicationState loadDefault,
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
            _flows = flows;
            _config = config;
            _fsm = new FiniteStateMachine(new IState[]
            {
                initialize,
                login,
                holoLogin,
                signOut,
                guest,
                orientation,
                mobileAr,
                userProfile,
                load,
                loadDefault,
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

        /// <inheritdoc />
        public override void Start()
        {
            Subscribe<Void>(
                MessageTypes.APPLICATION_INITIALIZED,
                Messages_OnApplicationInitialized);

            _fsm.Change<InitializeApplicationState>(_config);
        }

        /// <inheritdoc />
        public override void Update(float dt)
        {
            base.Update(dt);

            _fsm.Update(dt);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            _fsm.Change(null);

            if (null != _flow)
            {
                _flow.Stop();
                _flow = null;
            }
        }
        
        /// <inheritdoc />
        public void ChangeState<T>(object context = null) where T : IState
        {
            _fsm.Change<T>(context);
        }

        /// <inheritdoc />
        public void ChangeFlow<T>() where T : IStateFlow
        {
            if (null != _flow)
            {
                _flow.Stop();
            }

            _flow = GetFlow<T>();

            if (null != _flow)
            {
                _flow.Start(this);
            }
        }

        /// <summary>
        /// Listens for messages that flows will use and passes them along to flows.
        /// </summary>
        /// <param name="messageTypes">Message types to listen to.</param>
        private void ListenForFlowMessages(params int[] messageTypes)
        {
            for (var i = 0; i < messageTypes.Length; i++)
            {
                // make local for access inside closure
                var messageType = messageTypes[i];
                
                Subscribe<Void>(messageType, _ =>
                {
                    if (null != _flow)
                    {
                        _flow.MessageReceived(messageType, Void.Instance);
                    }
                });
            }
        }

        /// <summary>
        /// Retrieves the flow for a type.
        /// </summary>
        private IStateFlow GetFlow<T>() where T : IStateFlow
        {
            for (int i = 0, len = _flows.Length; i < len; i++)
            {
                var flow = _flows[i];
                if (flow.GetType() == typeof(T))
                {
                    return flow;
                }
            }

            return null;
        }

        /// <summary>
        /// Called when application initialized message is published.
        /// </summary>
        /// <param name="_">Void.</param>
        private void Messages_OnApplicationInitialized(Void _)
        {
            Log.Info(this, "Application initialized.");

            switch (UnityEngine.Application.platform)
            {
                case RuntimePlatform.WebGLPlayer:
                {
                    ChangeFlow<WebStateFlow>();
                    break;
                }
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.Android:
                {
                    ChangeFlow<MobileLoginStateFlow>();
                    break;
                }
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                {
                    ChangeFlow<HmdStateFlow>();
                    break;
                }
                default:
                {
                    Log.Warning(this, "Unknown platform. Defaulting to mobile flow.");
                    
                    ChangeFlow<MobileLoginStateFlow>();
                    break;
                }
            }
        }
    }
}