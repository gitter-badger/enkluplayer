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
        /// Displays load progress.
        /// </summary>
        private readonly ILoadProgressManager _progress;

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
        /// Id for load progress indicator.
        /// </summary>
        private uint _progressIndicatorId;

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
            IAssetPoolManager pools,
            ILoadProgressManager progress)
        {
            _assets = assets;
            _pools = pools;
            _progress = progress;
        }
        
        /// <inheritdoc cref="IContentAssembler"/>
        public void Setup(Vec3 transformPosition, string assetId)
        {
            WatchMainAsset(transformPosition, assetId);
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

            _progress.HideIndicator(_progressIndicatorId);
        }

        /// <summary>
        /// Watches main asset changes.
        /// </summary>
        private void WatchMainAsset(Vec3 transformPosition, string assetId)
        {
            // get the corresponding asset
            _asset = _assets.Manifest.Asset(assetId);
            if (null == _asset)
            {
                Log.Warning(this,
                    "Could not find Asset for content {0}.",
                    assetId);

                return;
            }

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
                _progressIndicatorId = _progress.ShowIndicator(
                    transformPosition,
                    _bounds.Min,
                    _bounds.Max,
                    _asset.Progress);
            }

            // watch for asset reloads
            // TODO: No way to see if asset load failed!
            _unwatch = _asset.Watch<GameObject>(value =>
            {
                Log.Info(this, "Asset loaded.");

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
    }
}