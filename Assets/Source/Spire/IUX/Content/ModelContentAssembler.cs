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
        /// Manages app data.
        /// </summary>
        private readonly IAppDataManager _appData;

        /// <summary>
        /// Manages pooling.
        /// </summary>
        private readonly IAssetPoolManager _pools;

        /// <summary>
        /// Loads assets.
        /// </summary>
        private readonly IAssetManager _assets;

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
        /// Loads materials.
        /// </summary>
        private readonly MaterialLoader _materialLoader;

        /// <summary>
        /// Data.
        /// </summary>
        private ContentData _data;

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
            IAssetPoolManager pools)
        {
            _appData = appData;
            _assets = assets;
            _pools = pools;

            _materialLoader = new MaterialLoader(appData, assets);
        }
        
        /// <inheritdoc cref="IContentAssembler"/>
        public void Setup(ContentData data)
        {
            _data = data;

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
        }

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

            // watch to unload
            _asset.OnRemoved += Asset_OnRemoved;

            // asset might already be loaded!
            var prefab = _asset.As<GameObject>();
            if (null != prefab)
            {
                SetupInstance(prefab);
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

            // get a new one
            _instance = _pools.Get<GameObject>(value);

            // apply material
            ApplyMaterial(_instance, _materialLoader.Material);

            // asset is loaded
            if (null != OnAssemblyComplete)
            {
                OnAssemblyComplete(_instance);
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
            // ?
        }
    }
}