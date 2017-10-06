using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

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
        event Action<AssetData[]> OnAdded;

        /// <summary>
        /// Called when assets have bee updated.
        /// </summary>
        event Action<AssetData[]> OnUpdated;

        /// <summary>
        /// Called when assets have been removed.
        /// </summary>
        event Action<AssetData[]> OnRemoved;

        /// <summary>
        /// Initializes service.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Initialize();

        /// <summary>
        /// Uninitializes the service.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Uninitialize();
    }
}