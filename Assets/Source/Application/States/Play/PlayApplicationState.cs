﻿using System;
using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
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
        private readonly IDesignController _design;

        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        private readonly ApplicationConfig _appConfig;
        
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
            IMessageRouter messages,
            IScriptRequireResolver resolver,
            IAppController app,
            IDesignController design,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _messages = messages;
            _resolver = resolver;
            _app = app;
            _design = design;
            _appConfig = config;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            Log.Info(this, "PlayApplicationState::Enter()");
            
            _enterTime = DateTime.Now;
            _statusCleared = false;
            _messages.Publish(
                MessageTypes.STATUS,
                NetworkUtils.GetNetworkSummary());
            
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

            // teardown app
            _app.Uninitialize();

            // teardown designer
            if (null != _design)
            {
                _design.Teardown();
            }
            
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

            // initialize with app id
            _app
                .Initialize(_appConfig.Play.AppId, config)
                .OnSuccess(_ =>
                {
                    Log.Info(this, "AppController initialized.");
                    
                    // TODO: Only if some condition is true
                    _design.Setup(config, _app);
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