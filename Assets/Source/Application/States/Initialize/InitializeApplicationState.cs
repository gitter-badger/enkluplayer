using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.AR;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.BLE;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

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
        private readonly IHashProvider _hashMethod;
        private readonly IAssetManager _assets;
        private readonly BleServiceConfiguration _bleConfig;
        private readonly IBleService _ble;

        /// <summary>
        /// App config.
        /// </summary>
        private ApplicationConfig _appConfig;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializeApplicationState(
            IMessageRouter messages,
            IHttpService http,
            IBootstrapper bootstrapper,
            IHashProvider hashMethod,
            IAssetManager assets,
            BleServiceConfiguration bleConfig,
            IBleService ble,
            IImageLoader imageLoader)
        {
            _messages = messages;
            _http = http;
            _bootstrapper = bootstrapper;
            _hashMethod = hashMethod;
            _assets = assets;
            _bleConfig = bleConfig;
            _ble = ble;

            imageLoader.ReplaceProtocol(
                "assets",
                "https://assets.enklu.com:9091");
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            _appConfig = (ApplicationConfig) context;

            // ble
            _ble.Setup(_bleConfig);
            
            // setup http
            var env = _appConfig.Network.Environment(_appConfig.Network.Current);
            _http.UrlBuilder.FromUrl(env.Url);

            // setup assets
            // TODO: factory
            var loader = new StandardAssetLoader(
                _appConfig.Network,
                _bootstrapper,
                new StandardAssetBundleCache(
                    _bootstrapper,
                    _hashMethod,
                    Path.Combine(
                        UnityEngine.Application.persistentDataPath,
                        "Bundles")), 
                new UrlBuilder
                {
                    BaseUrl = "assets.enklu.com",
                    Port = 9091,
                    Protocol = "https"
                });

            // reset assets
            _assets.Uninitialize();
            
            // wait for tasks to finish
            var tasks = new List<IAsyncToken<Void>>
            {
                _assets.Initialize(new AssetManagerConfiguration
                {
                    Loader = loader,
                    Queries = new StandardQueryResolver()
                })
            };
            
            Async
                .All(tasks.ToArray())
                .OnSuccess(_ =>
                {
                    _messages.Publish(
                        MessageTypes.APPLICATION_INITIALIZED,
                        Void.Instance);
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