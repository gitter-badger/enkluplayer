using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Standard implementation of <c>IAssetLoader</c>.
    /// </summary>
    public class StandardAssetLoader : IAssetLoader
    {
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Builds URL.
        /// </summary>
        private readonly UrlBuilder _urls;

        /// <summary>
        /// URI to loader.
        /// </summary>
        private readonly Dictionary<string, AssetBundleLoader> _bundles = new Dictionary<string, AssetBundleLoader>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardAssetLoader(
            IBootstrapper bootstrapper,
            UrlBuilder urls)
        {
            _bootstrapper = bootstrapper;
            _urls = urls;
        }
        
        /// <summary>
        /// Loads an asset.
        /// </summary>
        /// <param name="data">The info to load.</param>
        /// <param name="progress">Outputs the load progress.</param>
        /// <returns></returns>
        public IAsyncToken<Object> Load(
            AssetData data,
            out LoadProgress progress)
        {
            var url = _urls.Url(data.Uri);

            AssetBundleLoader loader;
            if (!_bundles.TryGetValue(url, out loader))
            {
                loader = _bundles[url] = new AssetBundleLoader(
                    _bootstrapper,
                    url);
                loader.Load();
            }

            return loader.Asset(data.AssetName, out progress);
        }
    }
}