using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
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
    }
}