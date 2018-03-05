using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Passthrough implementation for non-AR platforms.
    /// </summary>
    public class PassthroughWorldAnchorProvider : IWorldAnchorProvider
    {
        /// <inheritdoc />
        public IAsyncToken<byte[]> Export(GameObject gameObject)
        {
            return new AsyncToken<byte[]>(new byte[0]);
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Import(GameObject gameObject, byte[] bytes)
        {
            return new AsyncToken<Void>(Void.Instance);
        }
    }
}