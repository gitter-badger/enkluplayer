using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

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
        /// For Http requests.
        /// </summary>
        private readonly IHttpService _http;

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
            IHttpService http,

            InitializeApplicationState initialize,
            EditApplicationState edit,
            PreviewApplicationState preview)
        {
            _host = host;
            _messages = messages;
            _http = http;

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

                    _host.Ready();
                });

            _messages.Subscribe(
                MessageTypes.AUTHORIZED,
                (message, unsub) =>
                {
                    var authorizedMessage = (AuthorizedEvent) message;

                    Log.Info(this, "Application authorized : " + authorizedMessage);

                    // setup http service
                    _http.UrlBuilder.Replacements.Add(Tuple.Create(
                        "userId",
                        authorizedMessage.profile.id));
                    _http.Headers.Add(Tuple.Create(
                        "Authorization",
                        string.Format("Bearer {0}", authorizedMessage.credentials.token)));

                    // demo
                    _states.Change<EditApplicationState>();
                });

            _messages.Subscribe(
                MessageTypes.PREVIEW,
                (message, unsub) =>
                {
                    Log.Info(this, "Preview requested.");

                    _states.Change<PreviewApplicationState>();
                });
        }
    }
}