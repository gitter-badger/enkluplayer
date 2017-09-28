using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Root application.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// The host.
        /// </summary>
        private readonly IApplicationHost _host;

        /// <summary>
        /// For routing messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Controls application states.
        /// </summary>
        private readonly FiniteStateMachine _states;

        /// <summary>
        /// Creates a new Application.
        /// </summary>
        public Application(
            IApplicationHost host,
            IMessageRouter messages,

            InitializeApplicationState initialize,
            EditApplicationState edit,
            PreviewApplicationState preview,
            PlayApplicationState play)
        {
            _host = host;
            _messages = messages;

            _states = new FiniteStateMachine(new IState[]
            {
                initialize,
                edit,
                preview,
                play
            });
        }

        /// <summary>
        /// Initializes the application.
        /// </summary>
        public void Initialize()
        {
            // application-wide messages
            Subscribe();

            // move to the default application state
            _states.Change<InitializeApplicationState>();
        }
        
        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">The time since last time Update was called.</param>
        public void Update(float dt)
        {
            _states.Update(dt);
        }
        
        /// <summary>
        /// Subscribes to application events.
        /// </summary>
        private void Subscribe()
        {
            _messages.SubscribeOnce(
                MessageTypes.READY,
                _ =>
                {
                    Log.Info(this, "Application ready.");

                    _host.Ready();
                });

            _messages.Subscribe(
                MessageTypes.PREVIEW,
                _ =>
                {
                    Log.Info(this, "Preview requested.");

                    _states.Change<PreviewApplicationState>();
                });

            _messages.Subscribe(
                MessageTypes.EDIT,
                _ =>
                {
                    Log.Info(this, "Edit requested.");

                    _states.Change<EditApplicationState>();
                });

            _messages.Subscribe(
                MessageTypes.PLAY,
                _ =>
                {
                    Log.Info(this, "Play requested.");

                    _states.Change<PlayApplicationState>();
                });
        }
    }
}