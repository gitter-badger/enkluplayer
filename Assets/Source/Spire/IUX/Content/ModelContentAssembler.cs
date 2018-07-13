using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Assembler for models.
    /// </summary>
    public class ModelContentAssembler : IContentAssembler
    {
        /// <summary>
        /// Loads assets.
        /// </summary>
        private readonly IAssetManager _assets;

        /// <summary>
        /// Manages pooling.
        /// </summary>
        private readonly IAssetPoolManager _pools;
        
        /// <summary>
        /// Bounds of the asset.
        /// </summary>
        private AssetStatsBoundsData _bounds;

        /// <summary>
        /// Instance of Asset's prefab.
        /// </summary>
        private GameObject _instance;

        /// <summary>
        /// The Asset.
        /// </summary>
        private Asset _asset;
        
        /// <summary>
        /// An action to unsubscribe from <c>Asset</c> updates.
        /// </summary>
        private Action _unwatch;

        /// <summary>
        /// Outlines model bounds.
        /// </summary>
        private ModelLoadingOutline _outline;
        
        /// <inheritdoc />
        public Bounds Bounds
        {
            get
            {
                return new Bounds(
                    new Vector3(
                        _bounds.Min.x + (_bounds.Max.x - _bounds.Min.x) / 2f,
                        _bounds.Min.y + (_bounds.Max.y - _bounds.Min.y) / 2f,
                        _bounds.Min.z + (_bounds.Max.z - _bounds.Min.z) / 2f),
                    new Vector3(
                        _bounds.Max.x - _bounds.Min.x,
                        _bounds.Max.y - _bounds.Min.y,
                        _bounds.Max.z - _bounds.Min.z));
            }
        }

        /// <summary>
        /// Called when asset is setup.
        /// </summary>
        public event Action<GameObject> OnAssemblyComplete;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ModelContentAssembler(
            IAssetManager assets,
            IAssetPoolManager pools)
        {
            _assets = assets;
            _pools = pools;
        }
        
        /// <inheritdoc cref="IContentAssembler"/>
        public void Setup(Transform transform, string assetId)
        {
            WatchMainAsset(transform, assetId);
        }

        /// <inheritdoc cref="IContentAssembler"/>
        public void Teardown()
        {
            if (null != _instance)
            {
                _pools.Put(_instance);
                _instance = null;
            }

            if (null != _unwatch)
            {
                _unwatch();
                _unwatch = null;
            }

            if (null != _asset)
            {
                _asset.OnRemoved -= Asset_OnRemoved;
                _asset = null;
            }

            if (null != _outline)
            {
                UnityEngine.Object.Destroy(_outline);
            }
        }

        /// <summary>
        /// Watches main asset changes.
        /// </summary>
        private void WatchMainAsset(Transform transform, string assetId)
        {
            if (string.IsNullOrEmpty(assetId))
            {
                return;
            }

            // get the corresponding asset
            _asset = _assets.Manifest.Asset(assetId);
            if (null == _asset)
            {
                Log.Warning(this,
                    "Could not find Asset for content {0}.",
                    assetId);

                return;
            }

            // listen for asset load errors (make sure we only add once)
            _asset.OnLoadError -= Asset_OnLoadError;
            _asset.OnLoadError += Asset_OnLoadError;

            // setup bounds
            _bounds = _asset.Data.Stats.Bounds ?? new AssetStatsBoundsData
            {
                Min = -0.5f * Vec3.One,
                Max = 0.5f * Vec3.One
            };

            // watch to unload
            _asset.OnRemoved += Asset_OnRemoved;

            // asset might already be loaded!
            var prefab = _asset.As<GameObject>();
            if (null != prefab)
            {
                SetupInstance(prefab);
            }
            // otherwise, show a load indicator
            else
            {
                _outline = transform
                    .gameObject
                    .AddComponent<ModelLoadingOutline>();
                _outline.OnRetry += () =>
                {
                    _outline.HideError();

                    _asset.Load<GameObject>();
                };
                _outline.Init(Bounds);

                Log.Info(this, "WatcherAdded::{0}", _asset.Data.Guid);

                // asset might already have failed to load
                if (!string.IsNullOrEmpty(_asset.Error))
                {
                    Log.Info(this, "WatcherFailed::{0}", _asset.Data.Guid);
                    _outline.ShowError(_asset.Error);
                }
            }

            // watch for asset reloads
            _unwatch = _asset.Watch<GameObject>(value =>
            {
                Log.Info(this, "WatcherCalled::{0}", _asset.Data.Guid);

                SetupInstance(value);
            });

            // automatically reload
            _asset.AutoReload = true;
        }

        /// <summary>
        /// Creates an instance of the loaded asset and replaces the existing
        /// instance, if there is one.
        /// </summary>
        /// <param name="value">The GameObject that was loaded.</param>
        private void SetupInstance(GameObject value)
        {
            // put existing instance back
            if (null != _instance)
            {
                _pools.Put(_instance);
                _instance = null;
            }

            // shut off garbage
            RemoveBadComponents(value);

            // get a new one
            _instance = _pools.Get<GameObject>(value);

            // shut off outline
            if (null != _outline)
            {
                UnityEngine.Object.Destroy(_outline);
            }

            // asset is loaded
            if (null != OnAssemblyComplete)
            {
                OnAssemblyComplete(_instance);
            }
        }

        /// <summary>
        /// Removes bad components from a prefab.
        /// </summary>
        /// <param name="value">Value.</param>
        private void RemoveBadComponents(GameObject value)
        {
            var cameras = value.GetComponentsInChildren<Camera>(true);
            foreach (var camera in cameras)
            {
                camera.enabled = false;
            }
        }

        /// <summary>
        /// Called when the asset has been removed from the manifest.
        /// </summary>
        /// <param name="asset">The asset that has been removed.</param>
        private void Asset_OnRemoved(Asset asset)
        {
            Teardown();
        }

        /// <summary>
        /// Called when there's an asset load error.
        /// </summary>
        /// <param name="error">The error.</param>
        private void Asset_OnLoadError(string error)
        {
            if (null != _outline)
            {
                _outline.ShowError(error);
            }
        }
    }
}