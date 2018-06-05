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
        /// <param name="id">Unique id for this export. This same id must be passed to import.</param>
        /// <param name="gameObject">The root of the WorldAnchor.</param>
        /// <returns></returns>
        IAsyncToken<byte[]> Export(string id, GameObject gameObject);

        /// <summary>
        /// Imports an anchor from bytes.
        /// </summary>
        /// <param name="id">Id used to export.</param>
        /// <param name="gameObject">The root of the WorldAnchor.</param>
        /// <param name="bytes">The bytes to import.</param>
        /// <returns></returns>
        IAsyncToken<Void> Import(string id, GameObject gameObject, byte[] bytes);

        /// <summary>
        /// Disables anchor. Only affects gameObject if Import has already been
        /// called on anchor.
        /// </summary>
        /// <param name="gameObject">The gameObject.</param>
        void Disable(GameObject gameObject);
    }
}