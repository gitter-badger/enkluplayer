using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class PreviewApplicationState : IState
    {
        private readonly IAssetManager _assets;
        private readonly IInputManager _input;

        private IAsyncToken<GameObject> _load;
        private GameObject _instance;

        public PreviewApplicationState(
            IAssetManager assets,
            IInputManager input)
        {
            _assets = assets;
            _input = input;
        }

        public void Enter()
        {
            _load = _assets
                .Manifest
                .Reference("")
                .Load<GameObject>()
                .OnSuccess(instance =>
                {
                    _instance = Object.Instantiate(instance, Vector3.zero, Quaternion.identity);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load asset : {0}.", exception);
                });
        }

        public void Update(float dt)
        {
            _input.Update(dt);
        }

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