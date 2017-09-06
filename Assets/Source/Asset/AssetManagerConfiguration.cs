namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Configuration object for <c>AssetManager</c>.
    /// </summary>
    public class AssetManagerConfiguration
    {
        /// <summary>
        /// A loader implementation.
        /// </summary>
        public IAssetLoader Loader;

        /// <summary>
        /// For resolving queries.
        /// </summary>
        public IQueryResolver Queries;

        /// <summary>
        /// Optional service that automatically updates asset manifest.
        /// </summary>
        public IAssetUpdateService Service;

        /// <summary>
        /// True iff configuration is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return null != Loader && null != Queries;
        }
    }
}