using System;

namespace CreateAR.SpirePlayer.Test
{
    public class DummyAssetUpdateService : IAssetUpdateService
    {
        public event Action<AssetInfo[]> OnAdded;
        public event Action<AssetInfo[]> OnUpdated;
        public event Action<AssetInfo[]> OnRemoved;

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