using System;
using System.Collections;
using System.IO;
using CreateAR.Commons.Unity.Http;
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
        /// Controls design mode.
        /// </summary>
        private readonly DesignController _design;
        
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
            DesignController design)
        {
            _bootstrapper = bootstrapper;
            _files = files;
            _messages = messages;
            _resolver = resolver;
            _design = design;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
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
            
            // TODO: only with connection
            _design.Setup(config);
        }
    }
}