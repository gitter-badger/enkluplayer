using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public interface IAssetLoader
    {
        IAsyncToken<Object> Load(string url);
    }

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
                else
                {
                    return null;
                }
            }

            return _asset as T;
        }
    }
}