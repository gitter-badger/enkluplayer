using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class WebAssetUpdateService : IAssetUpdateService
    {
        
        public event Action<AssetInfo[]> OnAdded;
        public event Action<AssetInfo[]> OnUpdated;
        public event Action<AssetInfo[]> OnRemoved;

        public IAsyncToken<Void> Initialize()
        {
            return new AsyncToken<Void>(Void.Instance);
        }
    }
}