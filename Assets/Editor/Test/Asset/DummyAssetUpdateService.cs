using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyAssetUpdateService : IAssetUpdateService
    {
        public event Action<AssetData[]> OnAdded;
        public event Action<AssetData[]> OnUpdated;
        public event Action<AssetData[]> OnRemoved;

        public IAsyncToken<Void> Initialize()
        {
            return new AsyncToken<Void>(Void.Instance);
        }

        public void Added(params AssetData[] assets)
        {
            if (null != OnAdded)
            {
                OnAdded(assets);
            }
        }

        public void Updated(params AssetData[] assets)
        {
            if (null != OnUpdated)
            {
                OnUpdated(assets);
            }
        }

        public void Removed(params AssetData[] assets)
        {
            if (null != OnRemoved)
            {
                OnRemoved(assets);
            }
        }
    }
}