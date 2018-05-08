using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Cache implementation that does not actually cache.
    /// </summary>
    public class PassthroughWorldAnchorCache : IWorldAnchorCache
    {
        /// <inheritdoc />
        public bool Contains(string id, int version)
        {
            return false;
        }

        /// <inheritdoc />
        public void Save(string id, int version, byte[] bytes)
        {
            //
        }

        /// <inheritdoc />
        public IAsyncToken<byte[]> Load(string id, int version)
        {
            return new AsyncToken<byte[]>(new NotImplementedException());
        }
    }
}