using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Transport for txn requests.
    /// </summary>
    public interface IElementTxnTransport
    {
        IAsyncToken<Trellis.Messages.GetApp.Response> GetApp(string appId);
        IAsyncToken<Trellis.Messages.GetScene.Response> GetScene(string appId, string sceneId);
        IAsyncToken<Void> Request(string appId, string sceneId, ElementActionData[] actions);
    }
}