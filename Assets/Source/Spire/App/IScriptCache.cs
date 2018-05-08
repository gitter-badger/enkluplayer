using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object that caches scripts.
    /// </summary>
    public interface IScriptCache
    {
        /// <summary>
        /// True iff the cache contains an item.
        /// </summary>
        /// <param name="id">Unique id of the item in question.</param>
        /// <param name="version">Version of the script.</param>
        /// <returns></returns>
        bool Contains(string id, int version);

        /// <summary>
        /// Fire and forget asynchronous save function.
        /// </summary>
        /// <param name="id">The unique id to write.</param>
        /// <param name="version">The version to save.</param>
        /// <param name="value">The value to write.</param>
        void Save(string id, int version, string value);

        /// <summary>
        /// Asynchronously loads script for a unique id.
        /// </summary>
        /// <param name="id">Unique id of the item to load.</param>
        /// <param name="version">The version to load.</param>
        /// <returns></returns>
        IAsyncToken<string> Load(string id, int version);

        /// <summary>
        /// Deletes everything in this cache that's older than the DateTime
        /// passed in.
        /// </summary>
        /// <param name="cutoff">The cutoff-- everything updated since this is deleted.</param>
        void Purge(DateTime cutoff);
    }
}