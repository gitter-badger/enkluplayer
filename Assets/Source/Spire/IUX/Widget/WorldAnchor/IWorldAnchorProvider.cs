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

        /// <summary>
        /// Disables anchor. Only affects gameObject if Import has already been
        /// called on anchor.
        /// </summary>
        /// <param name="gameObject">The gameObject.</param>
        void Disable(GameObject gameObject);

        /// <summary>
        /// Enables anchor. Only affects gameObject if Import has already been
        /// called on anchor. This is also automatically called by Import.
        /// </summary>
        /// <param name="gameObject">The gameObject.</param>
        void Enable(GameObject gameObject);
    }
}