using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
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
            IInputManager input)
        {
            _assets = assets;
            _input = input;
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
            var reference = _assets.Manifest.Asset(@event.id);
            if (null == reference)
            {
                Log.Warning(
                    this,
                    "Could not find AssetReference with guid " + @event.id);
                return;
            }

            Log.Info(this, "Loading asset.");

            // load!
            _load = reference.Load<GameObject>();
            _load.OnSuccess(instance =>
                {
                    Log.Info(this, "Successfully loaded.");

                    _instance = Object.Instantiate(instance, Vector3.zero, Quaternion.identity);

                    Camera.main.transform.LookAt(_instance.transform);
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

            if (null != _instance)
            {
                Object.Destroy(_instance);
                _instance = null;
            }

            // unload scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
        }
    }
}