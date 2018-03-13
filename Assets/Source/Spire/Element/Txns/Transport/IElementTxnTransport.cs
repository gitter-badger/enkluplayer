using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Transport for txn requests.
    /// </summary>
    public interface IElementTxnTransport
    {
        /// <summary>
        /// Retrieves the app.
        /// </summary>
        /// <param name="appId">App id.</param>
        /// <returns></returns>
        IAsyncToken<Trellis.Messages.GetApp.Response> GetApp(string appId);

        /// <summary>
        /// Retrieves a scene.
        /// </summary>
        /// <param name="appId">App id.</param>
        /// <param name="sceneId">Scene id.</param>
        /// <returns></returns>
        IAsyncToken<Trellis.Messages.GetScene.Response> GetScene(string appId, string sceneId);

        /// <summary>
        /// Requests actions to be performed.
        /// </summary>
        /// <param name="id">Session unique id.</param>
        /// <param name="appId">App id.</param>
        /// <param name="sceneId">Scene id.</param>
        /// <param name="actions">The actions to request.</param>
        /// <returns></returns>
        IAsyncToken<Void> Request(uint id, string appId, string sceneId, ElementActionData[] actions);
    }
}