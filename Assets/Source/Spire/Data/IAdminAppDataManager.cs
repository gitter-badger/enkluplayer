namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Provides additional methods for editing data.
    /// </summary>
    public interface IAdminAppDataManager : IAppDataManager
    {
        /// <summary>
        /// Removes all previous data for a type and replaces it with the passed
        /// in data.
        /// </summary>
        /// <typeparam name="T">The type of data to replace.</typeparam>
        /// <param name="data">The data to replace with.</param>
        void Set<T>(params T[] data) where T : StaticData;

        /// <summary>
        /// Adds data.
        /// </summary>
        /// <typeparam name="T">The type of data to add.</typeparam>
        /// <param name="data">The data to add.</param>
        void Add<T>(params T[] data) where T : StaticData;

        /// <summary>
        /// Removes data.
        /// </summary>
        /// <typeparam name="T">The type of data to remove.</typeparam>
        /// <param name="data">The data to remove.</param>
        void Remove<T>(params T[] data) where T : StaticData;

        /// <summary>
        /// Updates data.
        /// </summary>
        /// <typeparam name="T">The type of data to update.</typeparam>
        /// <param name="data">The data to update.</param>
        void Update<T>(params T[] data) where T : StaticData;
    }
}