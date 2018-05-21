using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Updates environment data.
    /// </summary>
    public class EnvironmentUpdateService : ApplicationService
    {
        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly ApplicationConfig _config;
        
        /// <summary>
        /// Http service.
        /// </summary>
        private readonly IHttpService _http;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public EnvironmentUpdateService(
            ApplicationConfig config,
            MessageTypeBinder binder,
            IMessageRouter messages,
            IHttpService http)
            : base(binder, messages)
        {
            _config = config;
            _http = http;
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            RegisterUrlBuilders(_config.Network.Environment);

            Subscribe<EnvironmentInfoEvent>(
                MessageTypes.RECV_ENV_INFO,
                message =>
                {
                    // update ApplicationConfig
                    var env = _config.Network.Environment;
                    env.AssetsUrl = message.AssetBaseUrl;
                    env.ThumbsUrl = message.ThumbsUrl;
                    env.TrellisUrl = message.TrellisBaseUrl;
                    
                    RegisterUrlBuilders(env);
                });
        }

        /// <summary>
        /// Registers url builders using the latest environment data.
        /// </summary>
        /// <param name="env">Environment.</param>
        private void RegisterUrlBuilders(EnvironmentData env)
        {
            Log.Info(this, "Registering URLs against environment: {0}.", env);

            var trellisFormatter = new LoggedUrlFormatter();
            if (!trellisFormatter.FromUrl(env.TrellisUrl))
            {
                Log.Error(this, "Invalid trellis URL : " + env.TrellisUrl);
            }

            var assetsFormatter = new LoggedUrlFormatter();
            if (!assetsFormatter.FromUrl(env.BundlesUrl))
            {
                Log.Error(this, "Invalid assets URL : " + env.BundlesUrl);
            }

            var thumbsFormatter = new LoggedUrlFormatter();
            if (!thumbsFormatter.FromUrl(env.ThumbsUrl))
            {
                Log.Error(this, "Invalid thumbs URL : " + env.ThumbsUrl);
            }

            var urls = _http.Urls;
            urls.Register("trellis", trellisFormatter);
            urls.Register("assets", assetsFormatter);
            urls.Register("thumbs", thumbsFormatter);

            urls.Default = "trellis";

            // reapply
            _config.Network.Credentials.Apply(_http);
        }
    }
}