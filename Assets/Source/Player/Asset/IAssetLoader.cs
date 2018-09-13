using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Assets
{
    /// <summary>
    /// Describes an interface for loading assets.
    /// </summary>
    public interface IAssetLoader
    {
        /// <summary>
        /// Builds URLs.
        /// </summary>
        UrlFormatterCollection Urls { get; }

        /// <summary>
        /// Loads an asset.
        /// </summary>
        /// <param name="data">The <c>AssetInfo</c> to load the accompanying
        /// asset for.</param>
        /// <param name="progress">Progress on the load.</param>
        /// <returns></returns>
        IAsyncToken<Object> Load(AssetData data, out LoadProgress progress);

        /// <summary>
        /// Destroys the loader and everything in it.
        /// </summary>
        void Destroy();
    }
}