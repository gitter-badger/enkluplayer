using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    public class StandardAssetLoader : IAssetLoader
    {
        private readonly IBootstrapper _bootstrapper;
        private readonly UrlBuilder _urls;

        private readonly Dictionary<string, AssetBundleLoader> _bundles = new Dictionary<string, AssetBundleLoader>();

        public StandardAssetLoader(
            IBootstrapper bootstrapper,
            UrlBuilder urls)
        {
            _bootstrapper = bootstrapper;
            _urls = urls;
        }
        
        public IAsyncToken<Object> Load(AssetInfo info, out LoadProgress progress)
        {
            var url = _urls.Url(info.Uri);

            AssetBundleLoader loader;
            if (!_bundles.TryGetValue(url, out loader))
            {
                loader = _bundles[url] = new AssetBundleLoader(
                    _bootstrapper,
                    url);
                loader.Load();
            }

            return loader.Asset(info.AssetName, out progress);
        }
    }
}