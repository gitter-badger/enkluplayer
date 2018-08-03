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
        /// Initializes provider.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Initialize(IAppSceneManager scenes);

        /// <summary>
        /// Attempts to anchor an object.
        /// </summary>
        /// <param name="id">Unique id that was previously used for importing an anchor.</param>
        /// <param name="gameObject">The GameObject to anchor.</param>
        /// <returns></returns>
        IAsyncToken<Void> Anchor(string id, GameObject gameObject);

        /// <summary>
        /// Unanchors an object. Only affects gameObject if Anchor has already
        /// been called on anchor.
        /// </summary>
        /// <param name="gameObject">The gameObject.</param>
        void UnAnchor(GameObject gameObject);

        /// <summary>
        /// Clears all anchors and reloads them.
        /// </summary>
        void ClearAllAnchors();

        /// <summary>
        /// Exports an anchor into bytes.
        /// </summary>
        /// <param name="id">Unique id for this export. This same id must be passed to import.</param>
        /// <param name="gameObject">The GameObject that is currently anchored.</param>
        /// <returns></returns>
        IAsyncToken<byte[]> Export(string id, GameObject gameObject);

        /// <summary>
        /// Imports an anchor from bytes.
        /// </summary>
        /// <param name="id">Id used to export.</param>
        /// <param name="bytes">The bytes to import.</param>
        /// <param name="gameObject">The GameObject to anchor.</param>
        /// <returns></returns>
        IAsyncToken<Void> Import(string id, byte[] bytes, GameObject gameObject);
    }
}