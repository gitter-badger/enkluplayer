using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.EnkluPlayer
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
        /// Unsubscribes from env updates.
        /// </summary>
        private Action _unsubEnvUpdates;

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
                    env.BundlesUrl = message.BundlesUrl;
                    env.ThumbsUrl = message.ThumbsUrl;
                    env.TrellisUrl = message.TrellisBaseUrl;
                    env.ScriptsUrl = message.ScriptsUrl;
                    env.AnchorsUrl = message.AnchorsUrl;
                    
                    RegisterUrlBuilders(env);
                });

            _unsubEnvUpdates = _messages.Subscribe(
                MessageTypes.ENV_INFO_UPDATE,
                Messages_OnEnvInfoUpdate);
        }
        
        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            _unsubEnvUpdates();
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

            var meshCaptureFormatter = new LoggedUrlFormatter();
            if (!meshCaptureFormatter.FromUrl(env.TrellisUrl))
            {
                Log.Error(this, "Invalid trellis URL : " + env.TrellisUrl);
            }
            else
            {
                meshCaptureFormatter.Version = "";
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

            var scriptsFormatter = new LoggedUrlFormatter();
            if (!scriptsFormatter.FromUrl(env.ScriptsUrl))
            {
                Log.Error(this, "Invalid script URL : " + env.ScriptsUrl);
            }

            var anchorsFormatter = new LoggedUrlFormatter();
            if (!anchorsFormatter.FromUrl(env.AnchorsUrl))
            {
                Log.Error(this, "Invalid anchors URL : " + env.AnchorsUrl);
            }

            var urls = _http.Urls;
            urls.Register("trellis", trellisFormatter);
            urls.Register("meshcapture", meshCaptureFormatter);
            urls.Register("assets", assetsFormatter);
            urls.Register("thumbs", thumbsFormatter);
            urls.Register("scripts", scriptsFormatter);
            urls.Register("anchors", anchorsFormatter);

            urls.Default = "trellis";

            // reapply
            _config.Network.Credentials.Apply(_http);
        }

        /// <summary>
        /// Called when environment info is updated.
        /// </summary>
        /// <param name="obj">Environment info.</param>
        private void Messages_OnEnvInfoUpdate(object obj)
        {
            var data = (EnvironmentData) obj;

            // write environment data to disk
            ApplicationConfigCompositor.Overwrite(data);
            
            // register new environment
            RegisterUrlBuilders(data);
        }
    }
}