using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Caches world anchors.
    /// </summary>
    public interface IWorldAnchorCache
    {
        /// <summary>
        /// True iff the cache already contains the world anchor.
        /// </summary>
        /// <param name="id">The id of the world anchor.</param>
        /// /// <param name="version">Version.</param>
        /// <returns></returns>
        bool Contains(string id, int version);

        /// <summary>
        /// Saves the world anchor to disk.
        /// </summary>
        /// <param name="id">Id of the world anchor.</param>
        /// /// <param name="version">Version.</param>
        /// <param name="bytes">Bytes to save to disk.</param>
        void Save(string id, int version, byte[] bytes);

        /// <summary>
        /// Loads world anchor data from disk.
        /// </summary>
        /// <param name="id">Id of the world anchor.</param>
        /// <param name="version">Version.</param>
        /// <returns></returns>
        IAsyncToken<byte[]> Load(string id, int version);
    }
}