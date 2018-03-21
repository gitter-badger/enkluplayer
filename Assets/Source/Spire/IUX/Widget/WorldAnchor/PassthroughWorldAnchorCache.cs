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
        public bool Contains(string id)
        {
            return false;
        }

        /// <inheritdoc />
        public void Save(string id, byte[] bytes)
        {
            //
        }

        /// <inheritdoc />
        public IAsyncToken<byte[]> Load(string id)
        {
            return new AsyncToken<byte[]>(new NotImplementedException());
        }
    }
}