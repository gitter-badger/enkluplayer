using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.Assets
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
        /// Cache for bundles.
        /// </summary>
        private readonly IAssetBundleCache _cache;

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
            IAssetBundleCache cache,
            UrlBuilder urls)
        {
            _bootstrapper = bootstrapper;
            _cache = cache;
            _urls = urls;
        }

        /// <inheritdoc cref="IAssetLoader"/>
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
                    _cache,
                    url);
                loader.Load();
            }

            return loader.Asset(data.AssetName, out progress);
        }

        /// <inheritdoc cref="IAssetLoader"/>
        public void Destroy()
        {
            foreach (var pair in _bundles)
            {
                pair.Value.Destroy();
            }
            _bundles.Clear();
        }
    }
}