using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Trellis.Messages.GetApp;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class BridgeElementTxnTransport : IElementTxnTransport
    {
        public IAsyncToken<Response> GetApp(string appId)
        {
            throw new NotImplementedException();
        }

        public IAsyncToken<Trellis.Messages.GetScene.Response> GetScene(string appId, string sceneId)
        {
            throw new NotImplementedException();
        }

        public IAsyncToken<Void> Request(string appId, string sceneId, ElementActionData[] actions)
        {
            throw new NotImplementedException();
        }
    }
}