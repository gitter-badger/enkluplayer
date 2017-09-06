using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class AssetManager
    {
        private AssetManagerConfiguration _config;
        private AsyncToken<Void> _initializeToken;

        public AssetManifest Manifest { get; private set; }

        public IAsyncToken<Void> Initialize(AssetManagerConfiguration config)
        {
            if (null != _initializeToken)
            {
                return new AsyncToken<Void>(new Exception("Already initialized."));
            }

            if (!config.IsValid())
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

        private void Service_OnAdded(AssetInfo[] assetInfos)
        {
            Manifest.Add(assetInfos);
        }

        private void Service_OnUpdated(AssetInfo[] assetInfos)
        {
            Manifest.Update(assetInfos);
        }

        private void Service_OnRemoved(AssetInfo[] assetInfos)
        {
            //
        }
    }
}