using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Interface for manipulating world anchors.
    /// </summary>
    public interface IWorldAnchorProvider
    {
        /// <summary>
        /// Exports an anchor into bytes.
        /// </summary>
        /// <param name="gameObject">The root of the WorldAnchor.</param>
        /// <returns></returns>
        IAsyncToken<byte[]> Export(GameObject gameObject);

        /// <summary>
        /// Imports an anchor from bytes.
        /// </summary>
        /// <param name="gameObject">The root of the WorldAnchor.</param>
        /// <param name="bytes">The bytes to import.</param>
        /// <returns></returns>
        IAsyncToken<Void> Import(GameObject gameObject, byte[] bytes);
    }
}