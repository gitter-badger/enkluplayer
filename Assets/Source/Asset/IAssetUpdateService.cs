using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an optional service that can pump asset updates into the
    /// <c>AssetManager</c>.
    /// </summary>
    public interface IAssetUpdateService
    {
        /// <summary>
        /// Called when assets have been added.
        /// </summary>
        event Action<AssetInfo[]> OnAdded;

        /// <summary>
        /// Called when assets have bee updated.
        /// </summary>
        event Action<AssetInfo[]> OnUpdated;

        /// <summary>
        /// Called when assets have been removed.
        /// </summary>
        event Action<AssetInfo[]> OnRemoved;
    }
}