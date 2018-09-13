using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Assets
{
    /// <summary>
    /// Entry point into the asset system.
    /// </summary>
    public class AssetManager : IAssetManager
    {
        /// <summary>
        /// Configuration for assets.
        /// </summary>
        private AssetManagerConfiguration _config;

        /// <summary>
        /// Token used to track initialization.
        /// </summary>
        private AsyncToken<Void> _initializeToken;

        /// <inheritdoc cref="IAssetManager"/>
        public AssetManifest Manifest { get; private set; }

        /// <inheritdoc cref="IAssetManager"/>
        public IAsyncToken<Void> Initialize(AssetManagerConfiguration config)
        {
            if (null != _initializeToken)
            {
                return new AsyncToken<Void>(new Exception("Already initialized."));
            }

            if (null == config || !config.IsValid())
            {
                return new AsyncToken<Void>(new Exception("Invalid configuration."));
            }

            _initializeToken = new AsyncToken<Void>();

            _config = config;

            Manifest = new AssetManifest(
                _config.Queries,
                _config.Loader);
            
            _initializeToken.Succeed(Void.Instance);

            return _initializeToken.Token();
        }

        /// <inheritdoc cref="IAssetManager"/>
        public void Uninitialize()
        {
            if (null == _initializeToken)
            {
                return;
            }

            _initializeToken.Fail(new Exception("Uninitialized."));
            _initializeToken = null;
            
            Manifest.Destroy();
            _config.Loader.Destroy();
            _config = null;
        }
    }
}