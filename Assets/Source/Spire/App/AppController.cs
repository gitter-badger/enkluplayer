using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class AppController
    {
        private readonly IAppDataManager _appData;
        private readonly ISceneManager _scenes;

        public AppExecutionConfiguration Config { get; private set; }

        public AppController(
            IAppDataManager appData,
            ISceneManager scenes)
        {
            _appData = appData;
            _scenes = scenes;
        }

        public IAsyncToken<Void> Startup(AppExecutionConfiguration config)
        {
            if (null != Config)
            {
                throw new Exception("App already running.");
            }

            var token = new AsyncToken<Void>();

            Config = config;
            
            // ?

            return token;
        }

        public void Update(float dt)
        {
            
        }

        public IAsyncToken<Void> Teardown()
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }
    }
}