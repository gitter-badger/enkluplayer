using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Test
{
    public class TestSceneManager : IAppSceneManager
    {
        public string[] All { get; private set; }
        public IAsyncToken<Void> Initialize(string appId, IAppDataLoader appData)
        {
            return new AsyncToken<Void>(Void.Instance).OnFinally(_ =>
            {
                OnInitialized.Execute();
            });
        }

        public IAsyncToken<Void> Uninitialize()
        {
            throw new NotImplementedException();
        }

        public Element Root(string sceneId)
        {
            throw new NotImplementedException();
        }

        public Action OnInitialized { get; set; }
    }
}