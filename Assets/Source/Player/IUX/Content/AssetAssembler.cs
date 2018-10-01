using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Assets;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Assembles an asset.
    /// </summary>
    public class AssetAssembler : IAssetAssembler
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
        /// Configuration for play mode.
        /// </summary>
        private readonly PlayAppConfig _config;

        /// <summary>
        /// Bounds of the asset.
        /// </summary>
        private AssetStatsBoundsData _bounds;
        
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

        /// <inheritdoc />
        public GameObject Assembly { get; private set; }

        /// <inheritdoc />
        public event Action OnAssemblyUpdated;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AssetAssembler(
            IAssetManager assets,
            IAssetPoolManager pools,
            PlayAppConfig config)
        {
            _assets = assets;
            _pools = pools;
            _config = config;
        }
        
        /// <inheritdoc />
        public void Setup(Transform transform, string assetId)
        {
            if (null != _asset)
            {
                throw new Exception("AssetAssembler was asked to setup twice in a row without a Teardown in between.");
            }

            if (string.IsNullOrEmpty(assetId))
            {
                return;
            }

            // get the corresponding asset
            _asset = _assets.Manifest.Asset(assetId);
            if (null == _asset)
            {
                Log.Warning(this,
                    "Could not find Asset by id {0}.",
                    assetId);

                return;
            }

            // watch for asset reloads
            Log.Info(this, "[{0}] WATCH.", _asset.Data.Guid);
            _unwatch = _asset.Watch<GameObject>(SetupInstance);

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
            // if it's not loaded and we're in edit mode, add a loading outline
            else if (_config.Edit)
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

                // asset might already have failed to load
                if (!string.IsNullOrEmpty(_asset.Error))
                {
                    _outline.ShowError(_asset.Error);
                }
            }

            // automatically reload
            Log.Info(this, "[{0}] Setting up autoload.", _asset.Data.Guid);
            _asset.AutoReload = true;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            if (null != Assembly)
            {
                _pools.Put(Assembly);
                Assembly = null;
            }

            if (null != _unwatch)
            {
                Log.Info(this, "[{0}] UNWATCH.", _asset.Data.Guid);
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
        /// Creates an instance of the loaded asset and replaces the existing
        /// instance, if there is one.
        /// </summary>
        /// <param name="value">The GameObject that was loaded.</param>
        private void SetupInstance(GameObject value)
        {
            Log.Info(this, "Asset {0} loaded. Assembling.", _asset.Data.Guid);

            // put existing instance back
            if (null != Assembly)
            {
                _pools.Put(Assembly);
                Assembly = null;
            }

            // shut off garbage
            RemoveBadComponents(value);

            // get a new one
            Assembly = _pools.Get<GameObject>(value);

            // shut off outline
            if (null != _outline)
            {
                UnityEngine.Object.Destroy(_outline);
            }

            // dispatch update
            if (null != OnAssemblyUpdated)
            {
                OnAssemblyUpdated();
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