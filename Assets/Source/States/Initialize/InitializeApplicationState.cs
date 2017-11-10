using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.AR;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

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
        private readonly ArServiceConfiguration _config;
        private readonly IArService _ar;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializeApplicationState(
            IMessageRouter messages,
            IHttpService http,
            IBootstrapper bootstrapper,
            IAssetManager assets,
            ArServiceConfiguration config,
            IArService ar)
        {
            _messages = messages;
            _http = http;
            _bootstrapper = bootstrapper;
            _assets = assets;
            _config = config;
            _ar = ar;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            // ar
            _ar.Setup(_config);
            _ar.Camera = Camera.main;
            
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
                    _messages.Publish(MessageTypes.READY, Void.Instance);
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