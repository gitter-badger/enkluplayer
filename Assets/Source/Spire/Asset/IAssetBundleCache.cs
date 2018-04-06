using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer.Assets
{
    /// <summary>
    /// Caches asset bundles.
    /// </summary>
    public interface IAssetBundleCache
    {
        /// <summary>
        /// Readies the cache.
        /// </summary>
        void Initialize();

        /// <summary>
        /// True iff the cache contains the bundle.
        /// </summary>
        /// <param name="uri">Effectively the key of the bundle.</param>
        /// <returns></returns>
        bool Contains(string uri);
        
        /// <summary>
        /// Loads a bundle from the cache.
        /// </summary>
        /// <param name="uri">Key of the bundle.</param>
        /// <param name="progress">Load progress.</param>
        /// <returns></returns>
        IAsyncToken<AssetBundle> Load(string uri, out LoadProgress progress);
        
        /// <summary>
        /// Caches the bundles bytes.
        /// </summary>
        /// <param name="uri">The key.</param>
        /// <param name="bytes">The bytes of the bundle.</param>
        void Save(string uri, byte[] bytes);
    }
}