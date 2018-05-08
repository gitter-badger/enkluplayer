using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Simple file cache that does its job.
    /// </summary>
    public interface IDiskCache
    {
        /// <summary>
        /// True iff the cache contains an item.
        /// </summary>
        /// <param name="id">Unique id of the item in question.</param>
        /// <returns></returns>
        bool Contains(string id);

        /// <summary>
        /// Fire and forget asynchronous save function. Overwrites existing bytes, if any.
        /// </summary>
        /// <param name="id">The unique id to write.</param>
        /// <param name="bytes">The bytes to write.</param>
        void Save(string id, byte[] bytes);

        /// <summary>
        /// Asynchronously loads bytes for a unique id.
        /// </summary>
        /// <param name="id">Unique id of the item to load.</param>
        /// <returns></returns>
        IAsyncToken<byte[]> Load(string id);

        /// <summary>
        /// Deletes everything in this disk cache that's older than the DateTime
        /// passed in.
        /// </summary>
        /// <param name="cutoff">The cutoff-- everything updated since this is deleted.</param>
        void Purge(DateTime cutoff);
    }
}
