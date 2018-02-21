using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State that loads necessary data before progressing to play. This is used
    /// when the player is running without a connected editor to push it data.
    /// </summary>
    public class LoadAppApplicationState : IState
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// For Http.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoadAppApplicationState(
            ApplicationConfig config,
            IHttpService http,
            IMessageRouter messages)
        {
            _config = config;
            _http = http;
            _messages = messages;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            ApplyCredentials(_config.Network);

            // TODO: load assets

            // TODO: load scripts

            Log.Info(this, "App loaded, proceeding to play.");

            _messages.Publish(MessageTypes.PLAY);
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            
        }

        /// <summary>
        /// Applies credentials.
        /// </summary>
        private void ApplyCredentials(NetworkConfig config)
        {
            // setup http service
            var creds = config.Credentials(config.Current);
            _http.UrlBuilder.Replacements.Add(Commons.Unity.DataStructures.Tuple.Create(
                "userId",
                creds.UserId));
            _http.Headers.Add(Commons.Unity.DataStructures.Tuple.Create(
                "Authorization",
                string.Format("Bearer {0}", creds.Token)));
        }
    }
}
