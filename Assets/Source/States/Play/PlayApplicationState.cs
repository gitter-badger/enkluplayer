using System.Collections;
using System.IO;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using Jint.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the play state.
    /// </summary>
    public class PlayApplicationState : IState
    {
        /// <summary>
        /// Name of the playmode scene to load.
        /// </summary>
        private const string SCENE_NAME = "PlayMode";

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

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
            IBootstrapper bootstrapper,
            IFileManager files,
            IScriptRequireResolver resolver,
            AppController app)
        {
            _bootstrapper = bootstrapper;
            _files = files;
            _resolver = resolver;
            _app = app;

            _design = new DesignController();
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
            _bootstrapper.BootstrapCoroutine(WaitForScene(
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                    SCENE_NAME,
                    LoadSceneMode.Additive)));

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

        /// <summary>
        /// Waits for the scene to load.
        /// </summary>
        /// <param name="op">Load operation.</param>
        /// <returns></returns>
        private IEnumerator WaitForScene(AsyncOperation op)
        {
            yield return op;

            // TODO: only on connection
            _design.Setup();
        }
    }
}