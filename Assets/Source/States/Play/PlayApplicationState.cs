using System;
using CreateAR.Commons.Unity.Logging;
using Jint.Unity;
using UnityEngine.SceneManagement;

namespace CreateAR.SpirePlayer
{
    public class PlayApplicationState : IState
    {
        /// <summary>
        /// Name of the playmode scene to load.
        /// </summary>
        private const string SCENE_NAME = "PlayMode";

        /// <summary>
        /// Gets + sets files.
        /// </summary>
        private readonly IFileManager _files;

        /// <summary>
        /// Resolves script requires.
        /// </summary>
        private readonly IScriptRequireResolver _resolver;

        /// <summary>
        /// Manages App.
        /// </summary>
        private readonly AppController _app;

        /// <summary>
        /// Plays an App.
        /// </summary>
        public PlayApplicationState(
            IFileManager files,
            IScriptRequireResolver resolver,
            AppController app)
        {
            _files = files;
            _resolver = resolver;
            _app = app;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter()
        {
            _resolver.Initialize(
#if NETFX_CORE
                // reference by hand
#else
                AppDomain.CurrentDomain.GetAssemblies()
#endif
            );

            // load playmode scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(SCENE_NAME, LoadSceneMode.Additive);

            // configure
            _files.Register(
                FileProtocols.APP,
                new SystemXmlSerializer(),
                new LocalFileSystem("Assets/Data/App"));
            
            // TODO: pull off of ApplicationState
            var appName = "StaticContentDemo";

            // start app
            _app
                .Startup(new AppExecutionConfiguration
                {
                    AppName = appName
                })
                .OnSuccess(_ => Log.Info(this, "App successfully started."))
                .OnFailure(exception => Log.Error(this, "Could not start app : {0}.", exception));
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            _app.Update(dt);
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            _files.Unregister(FileProtocols.APP);

            // unload playmode scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
        }
    }
}