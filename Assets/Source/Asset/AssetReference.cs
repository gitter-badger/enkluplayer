using System;
using CreateAR.Commons.Unity.Async;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    public class AssetReference
    {
        private readonly IAssetLoader _loader;
        private Object _asset;

        public AssetInfo Info { get; private set; }

        public AssetReference(
            IAssetLoader loader,
            AssetInfo info)
        {
            _loader = loader;
            Info = info;
        }

        public T Asset<T>() where T : Object
        {
            return As<T>();
        }

        public IAsyncToken<T> Load<T>() where T : Object
        {
            var token = new AsyncToken<T>();

            _loader
                .Load(Info.Uri)
                .OnSuccess(asset =>
                {
                    _asset = asset;

                    token.Succeed(As<T>());
                })
                .OnFailure(token.Fail);

            return token;
        }

        public void Update(AssetInfo info)
        {
            if (Info.Guid != info.Guid)
            {
                throw new ArgumentException("Cannot change AssetReference guid.");
            }

            Info = info;

            _asset = null;
        }

        private T As<T>() where T : Object
        {
            if (null == _asset)
            {
                return null;
            }

            if (typeof(Component).IsAssignableFrom(typeof(T)))
            {
                var gameObject = _asset as GameObject;
                if (gameObject != null)
                {
                    return gameObject.GetComponent<T>();
                }

                return null;
            }

            return _asset as T;
        }

        public void Watch(Action<Action, AssetReference> callback)
        {
            
        }
    }
}