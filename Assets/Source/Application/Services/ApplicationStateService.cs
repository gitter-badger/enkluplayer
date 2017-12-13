using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

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
            MeshCaptureApplicationState meshCapture)
            HierarchyApplicationState hierarchy,
            BleSearchApplicationState ble)
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
                meshCapture
                hierarchy,
                ble
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

                    switch (_config.Mode)
                    {
                        case PlayMode.MeshCapture:
                        {
                            _states.Change<MeshCaptureApplicationState>();
                            return;
                        }
                        default:
                        {
                            _states.Change<WaitingForConnectionApplicationState>();
                            return;
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
                MessageTypes.MESHCAPTURE,
                _ =>
                {
                    Log.Info(this, "Message capture requested.");

                    _states.Change<MeshCaptureApplicationState>();
                });

            _states.Change<InitializeApplicationState>();
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