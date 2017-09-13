using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Initializes the application.
    /// </summary>
    public class InitializeApplicationState : IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMessageRouter _messages;
        private readonly IHttpService _http;
        private readonly IAssetManager _assets;
        private readonly IAssetUpdateService _assetUpdater;

        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializeApplicationState(
            IMessageRouter messages,
            IHttpService http,
            IAssetManager assets,
            IAssetUpdateService assetUpdater)
        {
            _messages = messages;
            _http = http;
            _assets = assets;
            _assetUpdater = assetUpdater;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter()
        {
            // setup http
            _http.UrlBuilder.Protocol = "http";
            _http.UrlBuilder.BaseUrl = "localhost";
            _http.UrlBuilder.Port = 9999;
            _http.UrlBuilder.Version = "v1";

            // setup assets
            _assets
                .Initialize(new AssetManagerConfiguration
                {
                    Loader = new StandardAssetLoader(new UrlBuilder
                    {
                        BaseUrl = "ec2-54-202-152-140.us-west-2.compute.amazonaws.com",
                        Port = 9091,
                        Protocol = "http",
                        Version = "v1"
                    }),
                    Queries = new StandardQueryResolver(),
                    Service = _assetUpdater
                })
                .OnSuccess(_ =>
                {
                    _messages.Publish(
                        MessageTypes.READY,
                        new ApplicationReadyEvent());
                })
                .OnFailure(exception =>
                {
                    // rethrow
                    throw exception;
                });
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            
        }
    }
}