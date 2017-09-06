using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// An object that represents a reference to a Unity asset.
    /// </summary>
    public class AssetReference
    {
        /// <summary>
        /// The <c>IAssetLoader</c> implementation with which to load assets.
        /// </summary>
        private readonly IAssetLoader _loader;

        /// <summary>
        /// The object returned from the loader.
        /// </summary>
        private Object _asset;

        /// <summary>
        /// True iff the asset should be autoloaded upon update.
        /// </summary>
        private bool _autoReload;

        /// <summary>
        /// A list of callbacks subscribed through Watch().
        /// </summary>
        private readonly List<Action> _refWatchers = new List<Action>();

        /// <summary>
        /// A list of callbacks subscribed through WatchAsset().
        /// </summary>
        private readonly List<Action> _assetWatchers = new List<Action>();

        /// <summary>
        /// The data object describing the object.
        /// </summary>
        public AssetInfo Info { get; private set; }

        /// <summary>
        /// True iff the asset that is currently loaded is not the most recent
        /// version.
        /// </summary>
        public bool IsAssetDirty { get; private set; }

        /// <summary>
        /// When set to true, <c>AssetInfo</c> updates will cause this object
        /// to automatically reload the asset.
        /// </summary>
        public bool AutoReload
        {
            get
            {
                return _autoReload;
            }
            set
            {
                if (_autoReload == value)
                {
                    return;
                }

                _autoReload = value;

                if (_autoReload)
                {
                    Load<Object>();
                }
            }
        }

        /// <summary>
        /// Creates an <c>AssetReference</c>.
        /// </summary>
        /// <param name="loader">An <c>IAssetLoader</c> implementation with which
        /// to load assets.</param>
        /// <param name="info">The data object this pertains to.</param>
        public AssetReference(
            IAssetLoader loader,
            AssetInfo info)
        {
            _loader = loader;

            Info = info;
            IsAssetDirty = true;
        }

        /// <summary>
        /// Retrieves the asset cast to a <c>T</c>.
        /// 
        /// If <c>T</c> is a <c>Component</c>, the component will be pulled off
        /// of the main <c>GameObject</c>.
        /// </summary>
        /// <typeparam name="T">The type to cast the asset as.</typeparam>
        /// <returns></returns>
        public T Asset<T>() where T : Object
        {
            return As<T>();
        }

        /// <summary>
        /// Loads the asset.
        /// </summary>
        /// <typeparam name="T">Type parameter.</typeparam>
        /// <returns></returns>
        public IAsyncToken<T> Load<T>() where T : Object
        {
            var token = new AsyncToken<T>();

            if (IsAssetDirty)
            {
                var info = Info;
                _loader
                    .Load(info)
                    .OnSuccess(asset =>
                    {
                        _asset = asset;
                        IsAssetDirty = info != Info;

                        token.Succeed(As<T>());

                        var watchers = _assetWatchers.ToArray();
                        for (int i = 0, len = watchers.Length; i < len; i++)
                        {
                            watchers[i]();
                        }
                    })
                    .OnFailure(token.Fail);
            }
            else
            {
                token.Succeed(As<T>());
            }

            return token;
        }
        
        /// <summary>
        /// Called then the underlying <c>AssetInfo</c> needs to be updated.
        /// This will force the asset to be dirty. If <c>AutoReload</c> is set
        /// to true, the asset will be reloaded.
        /// </summary>
        /// <param name="info">The updated <c>AssetInfo</c> object.</param>
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

            IsAssetDirty = true;

            var watchers = _refWatchers.ToArray();
            for (int i = 0, len = watchers.Length; i < len; i++)
            {
                watchers[i]();
            }

            if (_autoReload)
            {
                Load<Object>();
            }
        }

        /// <summary>
        /// Watches for changes to the <c>AssetInfo</c>.
        /// 
        /// The callback's first parameter is a delegate to unsubscribe.
        /// </summary>
        /// <param name="callback">A callback to call.</param>
        public void Watch(Action<Action, AssetReference> callback)
        {
            Action watcher = null;
            // ReSharper disable once AccessToModifiedClosure
            Action unwatcher = () => _refWatchers.Remove(watcher);
            watcher = () => callback(unwatcher, this);

            _refWatchers.Add(watcher);
        }

        /// <summary>
        /// Watches for changes to the <c>AssetInfo</c>.
        /// 
        /// The returned delegate unsubscribes the callback.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        /// <returns></returns>
        public Action Watch(Action<AssetReference> callback)
        {
            Action watcher = () => callback(this);
            _refWatchers.Add(watcher);

            return () => _refWatchers.Remove(watcher);
        }

        /// <summary>
        /// Watches for changes to the loaded asset.
        /// 
        /// The callback's first parameter is a delegate to unsubscribe.
        /// </summary>
        /// <typeparam name="T">The type of asset. This is effectively the same
        /// as calling Asset().</typeparam>
        /// <param name="callback">The callback to call.</param>
        public void WatchAsset<T>(Action<Action, T> callback) where T : Object
        {
            Action watcher = null;
            // ReSharper disable once AccessToModifiedClosure
            Action unwatcher = () => _assetWatchers.Remove(watcher);
            watcher = () => callback(unwatcher, As<T>());

            _assetWatchers.Add(watcher);
        }

        /// <summary>
        /// Watches for changes to the loaded asset.
        /// 
        /// The returned delegate is for unsubscribing.
        /// </summary>
        /// <typeparam name="T">The type to cast the asset to.</typeparam>
        /// <param name="callback">The callback to call.</param>
        /// <returns></returns>
        public Action WatchAsset<T>(Action<T> callback) where T : Object
        {
            Action watcher = () => callback(As<T>());
            _assetWatchers.Add(watcher);

            return () => _assetWatchers.Remove(watcher);
        }

        /// <summary>
        /// Retrieves the asset cast to a <c>T</c>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns></returns>
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
    }
}