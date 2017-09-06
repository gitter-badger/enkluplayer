using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    public class AssetReference
    {
        private readonly IAssetLoader _loader;
        private Object _asset;

        private readonly List<Action> _refWatchers = new List<Action>();
        private readonly List<Action> _assetWatchers = new List<Action>();

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

                    var watchers = _assetWatchers.ToArray();
                    for (int i = 0, len = watchers.Length; i < len; i++)
                    {
                        watchers[i]();
                    }
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

            if (info == Info)
            {
                return;
            }

            Info = info;

            _asset = null;

            var watchers = _refWatchers.ToArray();
            for (int i = 0, len = watchers.Length; i < len; i++)
            {
                watchers[i]();
            }
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
            Action watcher = null;
            Action unwatcher = () => _refWatchers.Remove(watcher);
            watcher = () => callback(unwatcher, this);

            _refWatchers.Add(watcher);
        }

        public Action Watch(Action<AssetReference> callback)
        {
            Action watcher = () => callback(this);
            _refWatchers.Add(watcher);

            return () => _refWatchers.Remove(watcher);
        }

        public void WatchAsset<T>(Action<Action, T> callback) where T : Object
        {
            Action watcher = null;
            Action unwatcher = () => _assetWatchers.Remove(watcher);
            watcher = () => callback(unwatcher, As<T>());

            _assetWatchers.Add(watcher);
        }

        public Action WatchAsset<T>(Action<T> callback) where T : Object
        {
            Action watcher = () => callback(As<T>());
            _assetWatchers.Add(watcher);

            return () => _assetWatchers.Remove(watcher);
        }
    }
}