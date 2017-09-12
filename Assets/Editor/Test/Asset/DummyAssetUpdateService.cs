using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyAssetUpdateService : IAssetUpdateService
    {
        public event Action<AssetInfo[]> OnAdded;
        public event Action<AssetInfo[]> OnUpdated;
        public event Action<AssetInfo[]> OnRemoved;

        public IAsyncToken<Void> Initialize()
        {
            return new AsyncToken<Void>(Void.Instance);
        }

        public void Added(params AssetInfo[] assets)
        {
            if (null != OnAdded)
            {
                OnAdded(assets);
            }
        }

        public void Updated(params AssetInfo[] assets)
        {
            if (null != OnUpdated)
            {
                OnUpdated(assets);
            }
        }

        public void Removed(params AssetInfo[] assets)
        {
            if (null != OnRemoved)
            {
                OnRemoved(assets);
            }
        }
    }
}