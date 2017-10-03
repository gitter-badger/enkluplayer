using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class WebAssetUpdateService : IAssetUpdateService
    {
        
        public event Action<AssetData[]> OnAdded;
        public event Action<AssetData[]> OnUpdated;
        public event Action<AssetData[]> OnRemoved;

        public IAsyncToken<Void> Initialize()
        {
            return new AsyncToken<Void>(Void.Instance);
        }
    }
}