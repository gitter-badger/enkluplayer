using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Messaging;

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
        private readonly IBootstrapper _bootstrapper;
        private readonly IAssetManager _assets;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializeApplicationState(
            IMessageRouter messages,
            IHttpService http,
            IBootstrapper bootstrapper,
            IAssetManager assets)
        {
            _messages = messages;
            _http = http;
            _bootstrapper = bootstrapper;
            _assets = assets;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            // setup http
            _http.UrlBuilder.Protocol = "http";
            _http.UrlBuilder.BaseUrl = "localhost";
            _http.UrlBuilder.Port = 9999;
            _http.UrlBuilder.Version = "v1";

            // setup assets
            var loader = new StandardAssetLoader(
                _bootstrapper,
                new UrlBuilder
                {
                    BaseUrl = "ec2-54-202-152-140.us-west-2.compute.amazonaws.com",
                    Port = 9091,
                    Protocol = "http"
                });

            _assets.Uninitialize();
            _assets
                .Initialize(new AssetManagerConfiguration
                {
                    Loader = loader,
                    Queries = new StandardQueryResolver()
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