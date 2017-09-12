using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class StandardAssetLoader : IAssetLoader
    {
        private readonly UrlBuilder _urls;

        public StandardAssetLoader(UrlBuilder urls)
        {
            _urls = urls;
        }

        public IAsyncToken<Object> Load(AssetInfo info)
        {
            var token = new AsyncToken<Object>();

            
            
            return token;
        }

        
    }
}