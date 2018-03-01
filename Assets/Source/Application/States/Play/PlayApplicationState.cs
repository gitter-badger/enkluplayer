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
        private readonly IAdminAppController _app;

        /// <summary>
        /// Controls design mode.
        /// </summary>
        private readonly DesignController _design;

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
            IAdminAppController app,
            ApplicationConfig config,
            DesignController design)
        {
            _bootstrapper = bootstrapper;
            _messages = messages;
            _resolver = resolver;
            _app = app;
            _appConfig = config;
            _design = design;
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

            _design.Teardown();
            
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
                .Initialize(_appConfig.Play.AppId, config)
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
                                _design.Setup();
                            })
                            .OnFailure(exception =>
                            {
                                Log.Error(this, "Could not create Scene!");
                            });
                    }
                    else
                    {
                        // TODO: only with connection
                        _design.Setup();
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