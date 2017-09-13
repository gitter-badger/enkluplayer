using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

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
        public void Enter()
        {
            // fake data
            const string guid = "ae67e232-9079-41d0-88df-73870998cfd7";
            _assets.Manifest.Add(new AssetInfo
            {
                Guid = guid,
                Uri = string.Format("/bundles/{0}/asset.bundle", guid),
                AssetName = "Asset"
            });

            var reference = _assets.Manifest.Reference(guid);
            if (null == reference)
            {
                Log.Warning(
                    this,
                    "Could not find AssetReference with guid " + guid);
                return;
            }

            _load = reference.Load<GameObject>();

            _load.OnSuccess(instance =>
                {
                    Log.Info(this, "Successfully loaded.");

                    _instance = Object.Instantiate(instance, Vector3.zero, Quaternion.identity);
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
        }
    }
}