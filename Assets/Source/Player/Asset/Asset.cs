﻿using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer.Assets
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
        /// Backing variable for <c>Progress</c> property.
        /// </summary>
        private readonly LoadProgress _progress = new LoadProgress();

        /// <summary>
        /// The object returned from the loader.
        /// </summary>
        private Object _asset;

        /// <summary>
        /// Token returned from loader.
        /// </summary>
        private IAsyncToken<Object> _loadToken;

        /// <summary>
        /// The data object describing the object.
        /// </summary>
        public AssetData Data { get; private set; }

        /// <summary>
        /// The version.
        /// </summary>
        public int Version { get; private set; }
        
        /// <summary>
        /// Progress of the load.
        /// </summary>
        public LoadProgress Progress { get { return _progress; } }
        
        /// <summary>
        /// Called when there is a load error.
        /// </summary>
        public event Action<string> OnLoadError;

        /// <summary>
        /// Asset error.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Creates an <c>AssetReference</c>.
        /// </summary>
        /// <param name="loader">An <c>IAssetLoader</c> implementation with which
        /// to load assets.</param>
        /// <param name="data">The data object this pertains to.</param>
        /// <param name="version">The version.</param>
        public Asset(
            IAssetLoader loader,
            AssetData data,
            int version)
        {
            _loader = loader;

            Data = data;
            Version = version;
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

            if (null == _loadToken)
            {
                _loadToken = _loader.Load(Data, Version, out progress);

                // chain to class instance
                progress.Chain(Progress);

                _loadToken
                    .OnSuccess(asset =>
                    {
                        _loadToken = null;
                        _asset = asset;

                        Error = string.Empty;
                        
                        var cast = As<T>();
                        if (null == cast)
                        {
                            token.Fail(new Exception(string.Format(
                                "Asset {0} was loaded, but could not be cast from {1} to {2}.",
                                Data.Guid,
                                _asset.GetType().Name,
                                typeof(T).Name)));
                            return;
                        }
                        
                        token.Succeed(cast);
                    })
                    .OnFailure(exception =>
                    {
                        Log.Info(this, "Could not load asset : {0} : {1}.",
                            Data,
                            exception);

                        _loadToken = null;
                        Error = exception.Message;

                        if (null != OnLoadError)
                        {
                            OnLoadError(Error);
                        }

                        token.Fail(exception);
                    });
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
        /// Unloads the asset.
        /// </summary>
        public void Unload()
        {
            if (null != _loadToken)
            {
                _loadToken.Abort();
                _loadToken = null;
            }

            Progress.Value = 0;

            _asset = null;
            Error = string.Empty;
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