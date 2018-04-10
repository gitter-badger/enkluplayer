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
        /// Network configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

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
            ApplicationConfig config,
            IBootstrapper bootstrapper,
            IAssetBundleCache cache,
            UrlBuilder urls)
        {
            _config = config;
            _bootstrapper = bootstrapper;
            _cache = cache;
            _urls = urls;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            _cache.Initialize();

            _urls.BaseUrl = "assets.enklu.com";
            _urls.Port = 9091;
            _urls.Protocol = "https";
        }

        /// <inheritdoc />
        public IAsyncToken<Object> Load(
            AssetData data,
            out LoadProgress progress)
        {
            var url = _urls.Url(data.Uri);

            AssetBundleLoader loader;
            if (!_bundles.TryGetValue(url, out loader))
            {
                loader = _bundles[url] = new AssetBundleLoader(
                    _config.Network,
                    _bootstrapper,
                    _cache,
                    url);
                loader.Load();
            }

            // AssetImportServer uses the Guid
            return loader.Asset(data.Guid, out progress);
        }

        /// <inheritdoc />
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