using System;

namespace CreateAR.SpirePlayer
{
    public interface IAssetUpdateService
    {
        event Action<AssetInfo[]> OnAdded;
        event Action<AssetInfo[]> OnUpdated;
        event Action<AssetInfo[]> OnRemoved;
    }
}