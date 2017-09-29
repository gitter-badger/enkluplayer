using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer;
using CreateAR.SpirePlayer.Test;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.Spire.Test
{
    public class DummyAssetManager : IAssetManager
    {
        public AssetManifest Manifest { get; private set; }

        public DummyAssetManager()
        {
            Manifest = new AssetManifest(
                new StandardQueryResolver(),
                new DummyAssetLoader());
        }

        public IAsyncToken<Void> Initialize(AssetManagerConfiguration config)
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }

        public void Uninitialize()
        {
            throw new System.NotImplementedException();
        }
    }
}