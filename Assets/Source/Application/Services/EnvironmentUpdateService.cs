using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Updates environment data.
    /// </summary>
    public class EnvironmentUpdateService : ApplicationService
    {
        /// <summary>
        /// Environment URI.
        /// </summary>
        private const string ENV_URI = "config://env.prefs";

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly ApplicationConfig _config;
        
        /// <summary>
        /// Http service.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Persistent data.
        /// </summary>
        private readonly IFileManager _files;

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
            IFileManager files,
            IMessageRouter messages,
            IHttpService http)
            : base(binder, messages)
        {
            _config = config;
            _files = files;
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
        /// Loads environment data.
        /// </summary>
        public IAsyncToken<Void> Load()
        {
            var token = new AsyncToken<Void>();

            _files
                .Get<EnvironmentData>(ENV_URI)
                .OnSuccess(file =>
                {
                    Log.Info(this, "Found environment preference data.");

                    RegisterUrlBuilders(file.Data);
                })
                .OnFinally(_ => token.Succeed(Void.Instance));

            return token;
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

            var urls = _http.Urls;
            urls.Register("trellis", trellisFormatter);
            urls.Register("meshcapture", meshCaptureFormatter);
            urls.Register("assets", assetsFormatter);
            urls.Register("thumbs", thumbsFormatter);

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

            RegisterUrlBuilders(data);

            _files
                .Set(ENV_URI, data)
                .OnSuccess(_ => Log.Info(this, "Successfully saved environment preferences."))
                .OnFailure(ex => Log.Error(this, "Could not save environment preferences : {0}.", ex));
        }
    }
}