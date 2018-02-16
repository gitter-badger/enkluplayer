using System;
using System.Collections.ObjectModel;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class ElementTxnResult
    {

    }

    public interface IElementTxnManager
    {
        IAsyncToken<Void> Initialize(string appId);
        IAsyncToken<Void> Uninitialize();

        void TrackScene(string sceneId);
        void UntrackScene(string sceneId);

        ElementTxnResult Request(ElementTxn txn);
    }

    public interface IElementTxnStore
    {
        uint Apply(ElementTxn txn);
        void Rollback(uint id);
        void Commit(uint id);
    }

    public class ElementTxnStore : IElementTxnStore
    {
        public uint Apply(ElementTxn txn)
        {
            throw new System.NotImplementedException();
        }

        public void Rollback(uint id)
        {
            throw new System.NotImplementedException();
        }

        public void Commit(uint id)
        {
            throw new System.NotImplementedException();
        }
    }
}