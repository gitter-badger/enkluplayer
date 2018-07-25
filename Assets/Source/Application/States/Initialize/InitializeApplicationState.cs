using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.BLE;
using CreateAR.SpirePlayer.IUX;
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
        private readonly IAssetManager _assets;
        private readonly IAssetLoader _assetLoader;
        private readonly IBleService _ble;
        private readonly IWorldAnchorProvider _anchors;
        private readonly BleServiceConfiguration _bleConfig;
        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializeApplicationState(
            IMessageRouter messages,
            IAssetManager assets,
            IAssetLoader assetLoader,
            IBleService ble,
            IWorldAnchorProvider anchors,
            BleServiceConfiguration bleConfig)
        {
            _messages = messages;
            _assets = assets;
            _assetLoader = assetLoader;
            _bleConfig = bleConfig;
            _ble = ble;
            _anchors = anchors;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            // ble
            _ble.Setup(_bleConfig);
            
            // reset assets
            _assets.Uninitialize();
            
            // wait for tasks to finish
            var tasks = new List<IAsyncToken<Void>>
            {
                // TODO: Move into service.
                _assets.Initialize(new AssetManagerConfiguration
                {
                    Loader = _assetLoader,
                    Queries = new StandardQueryResolver()
                }),
                _anchors.Initialize()
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