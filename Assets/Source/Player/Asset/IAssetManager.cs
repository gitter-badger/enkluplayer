using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer.Assets
{
    /// <summary>
    /// Interface for asset management.
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// The manifest, which holds all the AssetRefs.
        /// </summary>
        AssetManifest Manifest { get; }

        /// <summary>
        /// Initializes the manager.
        /// </summary>
        /// <param name="config">The configuration for this manager.</param>
        /// <returns></returns>
        IAsyncToken<Void> Initialize(AssetManagerConfiguration config);

        /// <summary>
        /// Uninitializes the manager.
        /// </summary>
        void Uninitialize();
    }
}