using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Authenticates requests.
    /// </summary>
    public interface IAppTxnAuthenticator
    {
        /// <summary>
        /// Requests actions to be performed.
        /// </summary>
        /// <param name="id">Session unique id.</param>
        /// <param name="appId">App id.</param>
        /// <param name="sceneId">Scene id.</param>
        /// <param name="actions">The actions to request.</param>
        /// <returns></returns>
        IAsyncToken<Void> Request(int id, string appId, string sceneId, ElementActionData[] actions);
    }
}