using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;

using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.Spire
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

            // load data
            _appData
                .Load(Config.AppName)
                .OnSuccess(data =>
                {
                    Log.Info(this, "Loaded App.");

                    // now create entry scene
                    _scenes
                        .Load("main")
                        .OnSuccess(scene =>
                        {
                            // start scene
                            scene.Startup();

                            token.Succeed(Void.Instance);
                        })
                        .OnFailure(token.Fail);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load {0} : {1}.", Config.AppName, exception);

                    token.Fail(exception);
                });

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