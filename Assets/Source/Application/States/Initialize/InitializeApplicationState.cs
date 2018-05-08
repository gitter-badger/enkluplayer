using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.AR;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.BLE;
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
        
        private readonly IBootstrapper _bootstrapper;
        private readonly IAssetManager _assets;
        private readonly IAssetLoader _assetLoader;
        private readonly IArService _ar;
        private readonly IBleService _ble;
        private readonly ArServiceConfiguration _arConfig;
        private readonly BleServiceConfiguration _bleConfig;
        private readonly UrlFormatterCollection _urls;

        /// <summary>
        /// App config.
        /// </summary>
        private ApplicationConfig _appConfig;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializeApplicationState(
            IMessageRouter messages,
            IBootstrapper bootstrapper,
            IAssetManager assets,
            IAssetLoader assetLoader,
            IArService ar,
            IBleService ble,
            ArServiceConfiguration arConfig,
            BleServiceConfiguration bleConfig,
            UrlFormatterCollection urls)
        {
            _messages = messages;
            _bootstrapper = bootstrapper;
            _assets = assets;
            _assetLoader = assetLoader;
            _arConfig = arConfig;
            _ar = ar;
            _bleConfig = bleConfig;
            _ble = ble;
            _urls = urls;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            _appConfig = (ApplicationConfig) context;

            // ble
            _ble.Setup(_bleConfig);
            
            // setup URL builders from environment
            var env = _appConfig.Network.Environment(_appConfig.Network.Current);

            var trellisFormatter = new LoggedUrlFormatter();
            if (!trellisFormatter.FromUrl(env.TrellisUrl))
            {
                Log.Error(this, "Invalid trellis URL : " + env.TrellisUrl);
            }

            var assetsFormatter = new LoggedUrlFormatter();
            if (!assetsFormatter.FromUrl(env.AssetsUrl))
            {
                Log.Error(this, "Invalid assets URL : " + env.AssetsUrl);
            }

            var thumbsFormatter = new LoggedUrlFormatter();
            if (!thumbsFormatter.FromUrl(env.ThumbsUrl))
            {
                Log.Error(this, "Invalid thumbs URL : " + env.ThumbsUrl);
            }

            _urls.Register("trellis", trellisFormatter);
            _urls.Register("assets", assetsFormatter);
            _urls.Register("thumbs", thumbsFormatter);

            // reset assets
            _assets.Uninitialize();
            
            // wait for tasks to finish
            var tasks = new List<IAsyncToken<Void>>
            {
                _assets.Initialize(new AssetManagerConfiguration
                {
                    Loader = _assetLoader,
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