using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Assembler for models.
    /// </summary>
    public class ModelContentAssembler : IContentAssembler
    {
        /// <summary>
        /// Manages app data.
        /// </summary>
        private readonly IAppDataManager _appData;

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

        /// <summary>
        /// Loads materials.
        /// </summary>
        private readonly MaterialLoader _materialLoader;

        /// <summary>
        /// Data.
        /// </summary>
        private ContentData _data;

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
            IAppDataManager appData,
            IAssetManager assets,
            IAssetPoolManager pools,
            ILoadProgressManager progress)
        {
            _appData = appData;
            _assets = assets;
            _pools = pools;
            _progress = progress;

            _materialLoader = new MaterialLoader(appData, assets);
        }
        
        /// <inheritdoc cref="IContentAssembler"/>
        public void Setup(ContentData data)
        {
            _data = data;

            if (null == _data)
            {
                return;
            }

            WatchMainAsset();

            var material = _appData.Get<MaterialData>(_data.MaterialId);
            if (null != material)
            {
                _materialLoader.OnLoaded += Material_OnLoaded;
                _materialLoader.Update(material);
            }
        }

        /// <inheritdoc cref="IContentAssembler"/>
        public void UpdateMaterialData(MaterialData data)
        {
            _materialLoader.Update(data);
        }

        /// <inheritdoc cref="IContentAssembler"/>
        public void Teardown()
        {
            _materialLoader.OnLoaded -= Material_OnLoaded;

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
        private void WatchMainAsset()
        {
            // get the corresponding asset
            _asset = Asset(_data);
            if (null == _asset)
            {
                Log.Warning(this,
                    "Could not find Asset for content {0}.",
                    _data);

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
                if (null == _asset.Data)
                {
                    Log.Warning(this, "Could not find AssetData for {0}.", _data);
                }
                else
                {
                    _progressIndicatorId = _progress.ShowIndicator(
                        _bounds.Min,
                        _bounds.Max,
                        _asset.Progress);
                }
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
            
            // apply material
            //ApplyMaterial(_instance, _materialLoader.Material);

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
        /// Retrieves the asset corresponding to the input <c>ContentData</c>.
        /// </summary>
        /// <param name="data">The <c>ContentData</c> to find <c>Asset</c> for.</param>
        /// <returns></returns>
        private Asset Asset(ContentData data)
        {
            var assetId = null != data.Asset
                ? data.Asset.AssetDataId
                : string.Empty;
            if (string.IsNullOrEmpty(assetId))
            {
                return null;
            }

            return _assets.Manifest.Asset(assetId);
        }

        /// <summary>
        /// Applies material to all renders on object.
        /// </summary>
        /// <param name="instance"><c>GameObject</c> instance.</param>
        /// <param name="material">Material to apply.</param>
        private void ApplyMaterial(GameObject instance, Material material)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            for (int i = 0, len = renderers.Length; i < len; i++)
            {
                renderers[i].material = material;
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
        /// Called when the material has been loaded.
        /// </summary>
        private void Material_OnLoaded()
        {
            // do nothing at the moment
        }
    }
}