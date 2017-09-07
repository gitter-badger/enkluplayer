using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class StandardAssetLoader : IAssetLoader
    {
        public IAsyncToken<Object> Load(AssetInfo info)
        {
            var token = new AsyncToken<Object>();

            return token;
        }
    }
}