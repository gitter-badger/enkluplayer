﻿using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State for previewing assets.
    /// </summary>
    public class PreviewApplicationState : IState
    {
        /// <summary>
        /// Name of the scene to load.
        /// </summary>
        private const string SCENE_NAME = "PreviewMode";

        private readonly IAssetManager _assets;
        private readonly IInputManager _input;
        private readonly FocusManager _focus;

        /// <summary>
        /// The token for AssetReference load.
        /// </summary>
        private IAsyncToken<GameObject> _load;

        /// <summary>
        /// The instantiated asset.
        /// </summary>
        private GameObject _instance;

        /// <summary>
        /// Input state.
        /// </summary>
        [Inject(NamedInjections.INPUT_STATE_DEFAULT)]
        public IState InputState { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PreviewApplicationState(
            IAssetManager assets,
            IInputManager input,
            FocusManager focus)
        {
            _assets = assets;
            _input = input;
            _focus = focus;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            // load scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                SCENE_NAME,
                LoadSceneMode.Additive);

            // input
            _input.ChangeState(InputState);

            var @event = (PreviewEvent) context;
            
            // retrieve reference
            var reference = _assets.Manifest.Asset(@event.Guid);
            if (null == reference)
            {
                Log.Warning(
                    this,
                    "Could not find AssetReference with guid " + @event.Guid);
                return;
            }

            Log.Info(this, "Loading asset.");

            // load!
            _load = reference.Load<GameObject>();
            _load.OnSuccess(instance =>
                {
                    Log.Info(this, "Successfully loaded.");

                    RemoveBadComponents(instance);

                    _instance = Object.Instantiate(instance, Vector3.zero, Quaternion.identity);

                    _focus.Focus(_instance);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load asset : {0}.", exception);
                });
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            _input.Update(dt);
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            if (null != _load)
            {
                _load.Abort();
            }

            _focus.Focus(null);

            if (null != _instance)
            {
                Object.Destroy(_instance);
                _instance = null;
            }

            // unload scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
        }

        /// <summary>
        /// Removes bad components from a prefab.
        /// </summary>
        /// <param name="value">Value.</param>
        private void RemoveBadComponents(GameObject value)
        {
            var cameras = value.GetComponentsInChildren<Camera>(true);
            foreach (var camera in cameras)
            {
                camera.enabled = false;
            }
        }
    }
}