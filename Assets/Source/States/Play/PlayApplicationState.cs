using System;
using System.IO;
using CreateAR.Commons.Unity.Logging;
using Jint.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    public class DesignController
    {
        private readonly PlayModeConfig _playConfig;

        private GameObject _current;

        public DesignController()
        {
            _playConfig = Object.FindObjectOfType<PlayModeConfig>();

            if (null == _playConfig)
            {
                throw new Exception("Could not find PlayModeConfig.");
            }
        }

        public void Setup()
        {
            _current = Object.Instantiate(_playConfig.SplashMenu);
        }

        public void Teardown()
        {

        }
    }

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
        /// Controls design mode.
        /// </summary>
        private readonly DesignController _design;

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
        public void Enter(object context)
        {
            var config = (ApplicationConfig) context;

            _resolver.Initialize(
#if NETFX_CORE
                // reference by hand
#else
                System.AppDomain.CurrentDomain.GetAssemblies()
#endif
            );

            // load playmode scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(SCENE_NAME, LoadSceneMode.Additive);

            // configure
            _files.Register(
                FileProtocols.APP,
                new JsonSerializer(),
                new LocalFileSystem(Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    "App")));
            
            // start app
            _app
                .Startup(new AppExecutionConfiguration
                {
                    AppName = config.Play.AppId
                })
                .OnSuccess(_ =>
                {
                    Log.Info(this, "App successfully started.");
                })
                .OnFailure(exception => Log.Error(this, "Could not start app : {0}.", exception));

            // TODO: only on connection
            _design.Setup();
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            _app.Update(dt);
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            _design.Teardown();

            _files.Unregister(FileProtocols.APP);

            // unload playmode scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
        }
    }
}