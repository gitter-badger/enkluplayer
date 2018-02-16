using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public interface IElementTxnManager
    {
        IAsyncToken<Void> Initialize(string appId);
        IAsyncToken<Void> Uninitialize();

        void TrackScene(string sceneId);
        void UntrackScene(string sceneId);

        void Request(ElementTxn txn);
    }
}