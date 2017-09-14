using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State for previewing assets.
    /// </summary>
    public class PreviewApplicationState : IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IApplicationState _state;
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
            IApplicationState state,
            IAssetManager assets,
            IInputManager input)
        {
            _state = state;
            _assets = assets;
            _input = input;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter()
        {
            try { 
                // input
                _input.ChangeState(InputState);

                // get guid
                string guid;
                if (!_state.Get(
                    "webgl.preview.asset.id",
                    out guid))
                {
                    Log.Error(this, "Could not find assetId.");
                    return;
                }

                // get uri
                string uri;
                if (!_state.Get(
                    "webgl.preview.asset.uri",
                    out uri))
                {
                    Log.Error(this, "Could not find asset uri.");
                    return;
                }

                Log.Info(this,
                    "Got info : {0}, {1}.",
                    guid,
                    uri);

                // add it to manifest
                // TODO: This should be builtin.
                _assets.Manifest.Add(new AssetInfo
                {
                    Guid = guid,
                    Uri = uri,
                    AssetName = "Asset"
                });

                Log.Info(this, "Added to manifest.");

                // retrieve reference
                var reference = _assets.Manifest.Reference(guid);
                if (null == reference)
                {
                    Log.Warning(
                        this,
                        "Could not find AssetReference with guid " + guid);
                    return;
                }

                Log.Info(this, "Got reference.");

                // load!
                _load = reference.Load<GameObject>();

                Log.Info(this, "Called load.");

                _load.OnSuccess(instance =>
                    {
                        Log.Info(this, "Successfully loaded.");

                        //_instance = Object.Instantiate(instance, Vector3.zero, Quaternion.identity);
                    })
                    .OnFailure(exception =>
                    {
                        Log.Error(this, "Could not load asset : {0}.", exception);
                    });
            }
            catch (Exception exception)
            {
                Log.Error(this, "Exception: " + exception);
            }
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
        }
    }
}