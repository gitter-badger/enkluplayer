using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.EnkluPlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Test
{
    public class TestSceneManager : IAppSceneManager
    {
        private Element _root;
        
        public string[] All
        {
            get { return new[] { "test" }; }
        }

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
            return _root;
        }

        public Action OnInitialized { get; set; }

        // For tests - Set what will be returned from Root()
        public void SetRoot(Element element)
        {
            _root = element;
        }
    }
}