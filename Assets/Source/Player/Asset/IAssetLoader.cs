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
        /// <param name="data">The <c>AssetData</c> to load the accompanying
        /// asset for.</param>
        /// <param name="progress">Progress on the load.</param>
        /// <returns></returns>
        IAsyncToken<Object> Load(AssetData data, int version, out LoadProgress progress);

        /// <summary>
        /// Clears the download queue.
        /// </summary>
        void ClearDownloadQueue();

        /// <summary>
        /// Destroys the loader and everything in it.
        /// </summary>
        void Destroy();
    }
}