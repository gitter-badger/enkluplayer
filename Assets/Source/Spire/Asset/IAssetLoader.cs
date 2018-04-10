using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer.Assets
{
    /// <summary>
    /// Describes an interface for loading assets.
    /// </summary>
    public interface IAssetLoader
    {
        /// <summary>
        /// Initializes the loader.
        /// </summary>
        void Initialize();

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