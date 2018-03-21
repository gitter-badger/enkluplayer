using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer.IUX
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
        /// <returns></returns>
        bool Contains(string id);

        /// <summary>
        /// Saves the world anchor to disk.
        /// </summary>
        /// <param name="id">Id of the world anchor.</param>
        /// <param name="bytes">Bytes to save to disk.</param>
        void Save(string id, byte[] bytes);

        /// <summary>
        /// Loads world anchor data from disk.
        /// </summary>
        /// <param name="id">Id of the world anchor.</param>
        /// <returns></returns>
        IAsyncToken<byte[]> Load(string id);
    }
}