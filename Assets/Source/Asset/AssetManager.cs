using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class AssetManagerConfiguration
    {
        public IAssetLoader Loader;
        public IQueryResolver Queries;

        public bool IsValid()
        {
            return null != Loader && null != Queries;
        }
    }
    
    public class AssetManager
    {
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

            Manifest = new AssetManifest(
                config.Queries,
                config.Loader);

            _initializeToken.Succeed(Void.Instance);

            return _initializeToken;
        }

        public void Uninitialize()
        {
            if (null == _initializeToken)
            {
                return;
            }

            _initializeToken.Abort();
            _initializeToken = null;
        }
    }
}