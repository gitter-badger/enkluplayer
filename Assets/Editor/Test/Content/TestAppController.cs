using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    public class TestAppController : IAppController
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public event Action OnReady;
        public event Action OnUnloaded;
        public IAppSceneManager Scenes { get; private set; }
        public IAsyncToken<Void> Load(PlayAppConfig config)
        {
            return new AsyncToken<Void>(Void.Instance).OnFinally(_ =>
            {
                OnReady.Execute();
            });
        }

        public void Unload()
        {
            OnUnloaded.Execute();
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Edit()
        {
            throw new NotImplementedException();
        }
    }
}