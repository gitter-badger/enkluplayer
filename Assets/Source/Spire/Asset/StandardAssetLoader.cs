using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.Assets
{
    public interface IAssetBundleCache
    {
        bool Contains(string uri);
        IAsyncToken<AssetBundle> Load(string uri);
        void Save(string uri, AssetBundle bundle);
    }

    public class StandardAssetBundleCache : IAssetBundleCache
    {
        public bool Contains(string uri)
        {
            return false;
        }

        public IAsyncToken<AssetBundle> Load(string uri)
        {
            throw new System.NotImplementedException();
        }
        
        public void Save(string uri, AssetBundle bundle)
        {
            
        }
    }
    
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
                    new StandardAssetBundleCache(),
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