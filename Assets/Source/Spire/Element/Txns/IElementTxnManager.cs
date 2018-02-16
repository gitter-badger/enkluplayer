using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an interface for an object that listens for scene changes as
    /// well as requests scene changes from server.
    /// </summary>
    public interface IElementTxnManager
    {
        /// <summary>
        /// Initializes the manager for an app.
        /// </summary>
        /// <param name="appId">The id of the app.</param>
        /// <returns></returns>
        IAsyncToken<Void> Initialize(string appId);

        /// <summary>
        /// Unintializes the manager.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Uninitialize();

        /// <summary>
        /// Watches a scene updates.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        void TrackScene(string sceneId);

        /// <summary>
        /// Stops watching a scene.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        void UntrackScene(string sceneId);

        /// <summary>
        /// Requests that
        /// </summary>
        /// <param name="txn"></param>
        void Request(ElementTxn txn);
    }
}