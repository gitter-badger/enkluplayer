﻿using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.Spire
{
    /// <summary>
    /// An object that represents a reference to a Unity asset.
    /// </summary>
    public class Asset
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
        /// A list of callbacks watching AssetData.
        /// </summary>
        private readonly List<Action> _dataWatchers = new List<Action>();

        /// <summary>
        /// A list of callbacks watching the loaded asset.
        /// </summary>
        private readonly List<Action> _watch = new List<Action>();

        /// <summary>
        /// The data object describing the object.
        /// </summary>
        public AssetData Data { get; private set; }

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
        /// <param name="data">The data object this pertains to.</param>
        public Asset(
            IAssetLoader loader,
            AssetData data)
        {
            _loader = loader;

            Data = data;
            IsAssetDirty = true;
        }

        /// <summary>
        /// Retrieves the asset cast to a <c>T</c>.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns></returns>
        public T As<T>() where T : Object
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

        /// <summary>
        /// Loads the asset.
        /// </summary>
        /// <typeparam name="T">The type of asset or component.</typeparam>
        /// <returns></returns>
        public IAsyncToken<T> Load<T>() where T : Object
        {
            LoadProgress progress;
            return Load<T>(out progress);
        }

        /// <summary>
        /// Loads the asset.
        /// </summary>
        /// <typeparam name="T">The type of asset or component.</typeparam>
        /// <param name="progress">Outputs load progress.</param>
        /// <returns></returns>
        public IAsyncToken<T> Load<T>(out LoadProgress progress) where T : Object
        {
            var token = new AsyncToken<T>();

            if (IsAssetDirty)
            {
                var info = Data;
                _loader
                    .Load(info, out progress)
                    .OnSuccess(asset =>
                    {
                        _asset = asset;
                        IsAssetDirty = info != Data;

                        token.Succeed(As<T>());

                        var watchers = _watch.ToArray();
                        for (int i = 0, len = watchers.Length; i < len; i++)
                        {
                            watchers[i]();
                        }
                    })
                    .OnFailure(token.Fail);
            }
            else
            {
                // load is complete
                progress = new LoadProgress
                {
                    Value = 1f
                };

                token.Succeed(As<T>());
            }

            return token;
        }
        
        /// <summary>
        /// Called then the underlying <c>AssetInfo</c> needs to be updated.
        /// This will force the asset to be dirty. If <c>AutoReload</c> is set
        /// to true, the asset will be reloaded.
        /// </summary>
        /// <param name="data">The updated <c>AssetInfo</c> object.</param>
        public void Update(AssetData data)
        {
            if (Data.Guid != data.Guid)
            {
                throw new ArgumentException("Cannot change AssetReference guid.");
            }

            if (data == Data)
            {
                return;
            }

            Data = data;

            IsAssetDirty = true;

            var watchers = _dataWatchers.ToArray();
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
        public void WatchData(Action<Action, Asset> callback)
        {
            Action watcher = null;
            // ReSharper disable once AccessToModifiedClosure
            Action unwatcher = () => _dataWatchers.Remove(watcher);
            watcher = () => callback(unwatcher, this);

            _dataWatchers.Add(watcher);
        }

        /// <summary>
        /// Watches for changes to the <c>AssetInfo</c>.
        /// 
        /// The returned delegate unsubscribes the callback.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        /// <returns></returns>
        public Action WatchData(Action<Asset> callback)
        {
            Action watcher = () => callback(this);
            _dataWatchers.Add(watcher);

            return () => _dataWatchers.Remove(watcher);
        }

        /// <summary>
        /// Watches for changes to the loaded asset.
        /// 
        /// The callback's first parameter is a delegate to unsubscribe.
        /// </summary>
        /// <typeparam name="T">The type of asset. This is effectively the same
        /// as calling Asset().</typeparam>
        /// <param name="callback">The callback to call.</param>
        public void Watch<T>(Action<Action, T> callback) where T : Object
        {
            Action watcher = null;
            // ReSharper disable once AccessToModifiedClosure
            Action unwatcher = () => _watch.Remove(watcher);
            watcher = () => callback(unwatcher, As<T>());

            _watch.Add(watcher);
        }

        /// <summary>
        /// Watches for changes to the loaded asset.
        /// 
        /// The returned delegate is for unsubscribing.
        /// </summary>
        /// <typeparam name="T">The type to cast the asset to.</typeparam>
        /// <param name="callback">The callback to call.</param>
        /// <returns></returns>
        public Action Watch<T>(Action<T> callback) where T : Object
        {
            Action watcher = () => callback(As<T>());
            _watch.Add(watcher);

            return () => _watch.Remove(watcher);
        }

        /// <summary>
        /// Useful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[AssetReference Info={0}]",
                Data);
        }
    }
}