using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Entry point into the asset system.
    /// </summary>
    public class AssetManager
    {
        /// <summary>
        /// Configuration for assets.
        /// </summary>
        private AssetManagerConfiguration _config;

        /// <summary>
        /// Token used to track initialization.
        /// </summary>
        private AsyncToken<Void> _initializeToken;

        /// <summary>
        /// The manifest, which holds all the AssetRefs.
        /// </summary>
        public AssetManifest Manifest { get; private set; }

        /// <summary>
        /// Initializes the manager.
        /// </summary>
        /// <param name="config">The configuration for this manager.</param>
        /// <returns></returns>
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

            if (null != _config.Service)
            {
                _config.Service.OnAdded += Service_OnAdded;
                _config.Service.OnUpdated += Service_OnUpdated;
                _config.Service.OnRemoved += Service_OnRemoved;
            }

            _initializeToken.Succeed(Void.Instance);

            return _initializeToken.Token();
        }

        /// <summary>
        /// Uninitializes the manager.
        /// </summary>
        public void Uninitialize()
        {
            if (null == _initializeToken)
            {
                return;
            }

            _initializeToken.Fail(new Exception("Uninitialized."));
            _initializeToken = null;

            if (null != _config.Service)
            {
                _config.Service.OnAdded -= Service_OnAdded;
                _config.Service.OnUpdated -= Service_OnUpdated;
                _config.Service.OnRemoved -= Service_OnRemoved;
            }

            _config = null;
        }

        /// <summary>
        /// Called by the <c>IAssetUpdateService</c> when an asset has been added.
        /// </summary>
        /// <param name="assetInfos">The asset that has been added.</param>
        private void Service_OnAdded(AssetInfo[] assetInfos)
        {
            Manifest.Add(assetInfos);
        }

        /// <summary>
        /// Called by the <c>IAssetUpdateService</c> when an asset has been updated
        /// </summary>
        /// <param name="assetInfos">The asset that has been updated.</param>
        private void Service_OnUpdated(AssetInfo[] assetInfos)
        {
            Manifest.Update(assetInfos);
        }

        /// <summary>
        /// Called by the <c>IAssetUpdateService</c> when an asset has been removed.
        /// </summary>
        /// <param name="assetInfos">The asset that has been removed.</param>
        private void Service_OnRemoved(AssetInfo[] assetInfos)
        {
            throw new NotImplementedException();
        }
    }
}