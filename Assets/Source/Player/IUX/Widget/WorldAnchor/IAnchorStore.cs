using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for manipulating world anchors.
    /// </summary>
    public interface IAnchorStore
    {
        /// <summary>
        /// Initializes provider.
        /// </summary>
        IAsyncToken<Void> Setup(
            IElementTxnManager txns,
            IAppSceneManager scenes);

        /// <summary>
        /// Tears down the store.
        /// </summary>
        void Teardown();

        /// <summary>
        /// Anchors an object, asynchronously.
        /// </summary>
        /// <param name="id">Unique id of an anchor.</param>
        /// <param name="version">Anchor version.</param>
        /// <param name="gameObject">The GameObject to anchor.</param>
        void Anchor(string id, int version, GameObject gameObject);

        /// <summary>
        /// Un-anchors an object. Only affects gameObject if Anchor has already
        /// been called on anchor.
        /// </summary>
        /// <param name="gameObject">The gameObject.</param>
        void UnAnchor(GameObject gameObject);
        
        /// <summary>
        /// Exports and uploads an anchor.
        /// </summary>
        /// <param name="id">Unique id for this export. This same id must be passed to import.</param>
        /// <param name="version">Anchor version.</param>
        /// <param name="gameObject">The GameObject that is currently anchored.</param>
        IAsyncToken<Void> Export(string id, int version, GameObject gameObject);

        /// <summary>
        /// Clears all anchors and reloads them.
        /// </summary>
        void ClearAllAnchors();

        /// <summary>
        /// Retrieves the anchor status for a <c>GameObject</c>.
        /// </summary>
        /// <param name="gameObject">The object to retrieve status for.</param>
        /// <returns></returns>
        WorldAnchorStatus Status(GameObject gameObject);
    }
}