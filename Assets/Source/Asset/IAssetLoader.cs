using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an interface for loading assets.
    /// </summary>
    public interface IAssetLoader
    {
        /// <summary>
        /// Loads an asset.
        /// </summary>
        /// <param name="info">The <c>AssetInfo</c> to load the accompanying
        /// asset for.</param>
        /// <returns></returns>
        IAsyncToken<Object> Load(AssetInfo info);
    }
}