using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
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
            IBridge bridge,
            IMessageRouter messages,
            ApplicationConfig config,

            InitializeApplicationState initialize,
            WaitingForConnectionApplicationState wait,
            EditApplicationState edit,
            PreviewApplicationState preview,
            PlayApplicationState play,
            HierarchyApplicationState hierarchy,
            BleSearchApplicationState ble,
            InstaApplicationState insta,
            // TODO: find a different pattern to do this
#if NETFX_CORE
            MeshCaptureApplicationState meshCapture,
#endif
            ToolModeApplicationState tools)
            : base(bridge, messages)
        {
            _config = config;
            _states = new FiniteStateMachine(new IState[]
            {
                initialize,
                wait,
                edit,
                preview,
                play,
                hierarchy,
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
                MessageTypes.READY,
                _ =>
                {
                    Log.Info(this, "Application ready.");

                    var mode = ApplicationMode.WaitForConnection;
                    try
                    {
                        mode = (ApplicationMode) Enum.Parse(
                            typeof(ApplicationMode),
                            _config.Mode);
                    }
                    catch (Exception exception)
                    {
                        Log.Warning(this,
                            "Could not parse mode in ApplicationConfig : {0}.",
                            exception);
                    }

                    switch (mode)
                    {
                        case ApplicationMode.None:
                        {
                            break;
                        }
                        case ApplicationMode.Tools:
                        {
                            _states.Change<ToolModeApplicationState>();
                            break;
                        }
                        case ApplicationMode.WaitForConnection:
                        {
                            _states.Change<WaitingForConnectionApplicationState>();
                            break;
                        }
                        case ApplicationMode.Play:
                        {
                            _states.Change<PlayApplicationState>(_config);
                            break;
                        }
                        case ApplicationMode.Insta:
                        {
                            _states.Change<InstaApplicationState>();
                            break;
                        }
                    }
                });

            Subscribe<PreviewEvent>(
                MessageTypes.PREVIEW,
                @event =>
                {
                    Log.Info(this, "Preview requested.");

                    _states.Change<PreviewApplicationState>(@event);
                });

            Subscribe<Void>(
                MessageTypes.EDIT,
                _ =>
                {
                    Log.Info(this, "Edit requested.");

                    _states.Change<EditApplicationState>();
                });

            Subscribe<Void>(
                MessageTypes.PLAY,
                _ =>
                {
                    Log.Info(this, "Play requested.");

                    _states.Change<PlayApplicationState>();
                });

            Subscribe<Void>(
                MessageTypes.HIERARCHY,
                _ =>
                {
                    Log.Info(this, "Hierarchy requested.");

                    _states.Change<HierarchyApplicationState>();
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
    }
}