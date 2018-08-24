using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Memory cache.
    /// </summary>
    public class MemoryScriptCache : IScriptCache
    {
        /// <summary>
        /// Cache.
        /// </summary>
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        /// <inheritdoc />
        public bool Contains(string id, int version)
        {
            return _cache.ContainsKey(Key(id, version));
        }

        /// <inheritdoc />
        public void Save(string id, int version, string value)
        {
            _cache[Key(id, version)] = value;
        }

        /// <inheritdoc />
        public IAsyncToken<string> Load(string id, int version)
        {
            string script;
            if (_cache.TryGetValue(Key(id, version), out script))
            {
                return new AsyncToken<string>(script);
            }

            return new AsyncToken<string>(new Exception("Not found."));
        }

        /// <inheritdoc />
        public void Purge(DateTime cutoff)
        {
            _cache.Clear();
        }

        /// <summary>
        /// Creates a deterministic key.
        /// </summary>
        /// <param name="id">Script id.</param>
        /// <param name="version">Version key.</param>
        /// <returns></returns>
        private string Key(string id, int version)
        {
            return id + "_" + version;
        }
    }
}