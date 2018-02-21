using System;
using System.Collections;
using System.IO;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using Jint.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

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
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Resolves script requires.
        /// </summary>
        private readonly IScriptRequireResolver _resolver;

        /// <summary>
        /// Manages app.
        /// </summary>
        private readonly IAppController _app;

        /// <summary>
        /// Controls design mode.
        /// </summary>
        private readonly DesignController _design;

        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        private ApplicationConfig _appConfig;
        
        /// <summary>
        /// Time at which state was entered.
        /// </summary>
        private DateTime _enterTime;

        /// <summary>
        /// True iff status has been cleared.
        /// </summary>
        private bool _statusCleared;

        /// <summary>
        /// Plays an App.
        /// </summary>
        public PlayApplicationState(
            IBootstrapper bootstrapper,
            IFileManager files,
            IMessageRouter messages,
            IScriptRequireResolver resolver,
            IAppController app,
            IElementFactory elements,
            IVoiceCommandManager voice)
        {
            _bootstrapper = bootstrapper;
            _files = files;
            _messages = messages;
            _resolver = resolver;
            _app = app;

            _design = new DesignController(elements, _app, voice);
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            Log.Info(this, "PlayApplicationState::Enter()");

            _appConfig = (ApplicationConfig) context;

            _enterTime = DateTime.Now;
            _statusCleared = false;
            _messages.Publish(
                MessageTypes.STATUS,
                WaitingForConnectionApplicationState.GetNetworkSummary());
            
            _resolver.Initialize(
#if NETFX_CORE
                // reference by hand
#else
                AppDomain.CurrentDomain.GetAssemblies()
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
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            if (!_statusCleared
                && DateTime.Now.Subtract(_enterTime).TotalSeconds > 5)
            {
                _messages.Publish(MessageTypes.STATUS, "");
                _statusCleared = true;
            }
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            Log.Info(this, "PlayApplicationState::Exit()");

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

            var config = Object.FindObjectOfType<PlayModeConfig>();
            if (null == config)
            {
                throw new Exception("Could not find PlayModeConfig.");
            }

            // initialize with hardcoded app id
            _app
                .Initialize(_appConfig.Play.AppId)
                .OnSuccess(_ =>
                {
                    Log.Info(this, "AppController initialized.");

                    // create a default propset if there isn't one
                    if (null == _app.Active)
                    {
                        Log.Info(this, "No active Scene, creating a default.");

                        _app
                            .Create()
                            .OnSuccess(scene =>
                            {
                                // TODO: only with connection
                                _design.Setup(config);
                            })
                            .OnFailure(exception =>
                            {
                                Log.Error(this, "Could not create Scene!");
                            });
                    }
                    else
                    {
                        // TODO: only with connection
                        _design.Setup(config);
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, string.Format(
                        "Could not initialize App : {0}.",
                        exception));
                });
        }
    }
}