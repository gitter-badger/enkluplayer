using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Assembler for models.
    /// </summary>
    public class DefaultContentAssembler : IAssetAssembler
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
        /// The element's schema.
        /// </summary>
        private ElementSchema _schema;

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

        private ElementSchemaProp<string> _assetDataId;
        private ElementSchemaProp<string> _materialId;

        /// <summary>
        /// Called when asset is setup.
        /// </summary>
        public event Action<GameObject> OnAssemblyComplete;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DefaultContentAssembler(
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
        public void Setup(ElementSchema schema)
        {
            _schema = schema;

            _assetDataId = _schema.Get<string>("asset.id");
            _assetDataId.OnChanged += Asset_OnChange;

            _materialId = _schema.Get<string>("materialId");
            _materialId.OnChanged += Material_OnChange;

            _materialLoader.OnLoaded += Material_OnLoaded;

            WatchMainAsset();
            UpdateMaterial();
        }

        /// <inheritdoc cref="IContentAssembler"/>
        public void Teardown()
        {
            _materialId.OnChanged -= Material_OnChange;
            _materialId = null;

            _assetDataId.OnChanged += Asset_OnChange;
            _assetDataId = null;

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

            _schema = null;
        }

        /// <summary>
        /// Watches the main asset for changes.
        /// </summary>
        private void WatchMainAsset()
        {
            // get the corresponding asset
            _asset = _assets.Manifest.Asset(_assetDataId.Value);
            if (null == _asset)
            {
                Log.Warning(this,
                    "Could not find Asset for element {0}.",
                    _assetDataId.Value);

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
            // otherwise, show a load indicator
            else
            {
                _progressIndicatorId = _progress.ShowIndicator(
                    _asset.Data.Stats.Bounds.Min,
                    _asset.Data.Stats.Bounds.Max,
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
        /// Updates a material.
        /// </summary>
        private void UpdateMaterial()
        {
            var material = _appData.Get<MaterialData>(_materialId.Value);
            _materialLoader.Update(material);
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

        /// <summary>
        /// Called when the material has been changed.
        /// </summary>
        /// <param name="prop">The material prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Material_OnChange(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateMaterial();
        }

        /// <summary>
        /// Called when the asset has been changed.
        /// </summary>
        /// <param name="prop">The asset prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Asset_OnChange(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            var schema = _schema;

            Teardown();
            Setup(schema);
        }
    }
}