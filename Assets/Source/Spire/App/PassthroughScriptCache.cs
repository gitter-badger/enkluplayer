using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Script cache that does nothing.
    /// </summary>
    public class PassthroughScriptCache : IScriptCache
    {
        /// <inheritdoc />
        public bool Contains(string id, int version)
        {
            return false;
        }

        /// <inheritdoc />
        public void Save(string id, int version, string value)
        {
            
        }

        /// <inheritdoc />
        public IAsyncToken<string> Load(string id, int version)
        {
            return new AsyncToken<string>(new NotImplementedException());
        }

        /// <inheritdoc />
        public void Purge(DateTime cutoff)
        {
            
        }
    }
}