﻿using System.Collections.Generic;
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
        /// URI to loader.
        /// </summary>
        private readonly Dictionary<string, AssetBundleLoader> _bundles = new Dictionary<string, AssetBundleLoader>();

        /// <inheritdoc />
        public UrlFormatterCollection Urls { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardAssetLoader(
            ApplicationConfig config,
            IBootstrapper bootstrapper,
            IAssetBundleCache cache,
            UrlFormatterCollection urls)
        {
            _config = config;
            _bootstrapper = bootstrapper;
            _cache = cache;

            Urls = urls;
        }

        /// <inheritdoc />
        public IAsyncToken<Object> Load(
            AssetData data,
            out LoadProgress progress)
        {
            // strip off file name
            var substrings = data.Uri.Split('/');
            var url = Urls.Url("assets://" + substrings[substrings.Length - 1]);

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