using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
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
        /// Unsubscribe actions.
        /// </summary>
        private readonly List<Action> _flowMessageUnsubs = new List<Action>();
        
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
            ApplicationStatePackage package)
            : base(binder, messages)
        {
            _config = config;
            _fsm = new FiniteStateMachine(package.States);
            _flows = package.Flows;
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

                // unsub
                for (var i = 0; i < _flowMessageUnsubs.Count; i++)
                {
                    _flowMessageUnsubs[i]();
                }
                _flowMessageUnsubs.Clear();
            }

            _flow = GetFlow<T>();

            if (null != _flow)
            {
                _flow.Start(this);
            }
        }

        /// <inheritdoc />
        public void ListenForFlowMessages(params int[] messageTypes)
        {
            for (var i = 0; i < messageTypes.Length; i++)
            {
                // make local for access inside closure
                var messageType = messageTypes[i];

                _flowMessageUnsubs.Add(_messages.Subscribe(
                    messageType,
                    message =>
                    {
                        if (null != _flow)
                        {
                            _flow.MessageReceived(messageType, message);
                        }
                    }));
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

            switch (_config.ParsedPlatform)
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