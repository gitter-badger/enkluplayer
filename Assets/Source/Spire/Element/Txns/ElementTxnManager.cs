using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an interface for an object that listens for scene changes as
    /// well as requests scene changes from server.
    /// </summary>
    public interface IElementTxnManager
    {
        IAsyncToken<Void> Initialize(string appId);
        IAsyncToken<Void> Uninitialize();

        void TrackScene(string sceneId);
        void UntrackScene(string sceneId);

        void Request(ElementTxn txn);
    }
}