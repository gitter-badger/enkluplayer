using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Root application.
    /// </summary>
    public class Application : IApplicationHostDelegate
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
            PreviewApplicationState preview)
        {
            _host = host;
            _messages = messages;

            _states = new FiniteStateMachine(new IState[]
            {
                initialize,
                edit,
                preview
            });
            
#if !UNITY_EDITOR && UNITY_WEBGL
            UnityEngine.WebGLInput.captureAllKeyboardInput = false;
#endif
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

        /// <inheritdoc cref="IApplicationHostDelegate"/>
        public void On(int messageType, object message)
        {
            // push to message router
            _messages.Publish(messageType, message);
        }

        /// <summary>
        /// Subscribes to application events.
        /// </summary>
        private void Subscribe()
        {
            _messages.SubscribeOnce(
                MessageTypes.READY,
                message =>
                {
                    Log.Info(this, "Application ready.");

                    _host.Ready(this);
                });

            _messages.Subscribe(
                MessageTypes.AUTHORIZED,
                (message, unsub) =>
                {
                    Log.Info(this, "Application authorized!");

                    // demo
                    _states.Change<PreviewApplicationState>();
                });

            _messages.Subscribe(
                MessageTypes.PREVIEW_ASSET,
                (message, unsub) =>
                {
                    _states.Change<PreviewApplicationState>();
                });
        }
    }
}