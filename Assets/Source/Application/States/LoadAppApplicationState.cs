using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    public class LoadAppApplicationState : IState
    {
        private readonly ApplicationConfig _config;
        private readonly IHttpService _http;
        private readonly IMessageRouter _messages;

        public LoadAppApplicationState(
            ApplicationConfig config,
            IHttpService http,
            IMessageRouter messages)
        {
            _config = config;
            _http = http;
            _messages = messages;
        }

        public void Enter(object context)
        {
            ApplyCredentials(_config.Network);

            // TODO: get assets

            // TODO: get scripts

            Log.Info(this, "App loaded, proceeding to play.");

            _messages.Publish(MessageTypes.PLAY);
        }

        public void Update(float dt)
        {
            
        }

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
