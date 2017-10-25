using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Widget that loads + holds an asset.
    /// </summary>
    public class Content : Widget
    {
        /// <summary>
        /// Loads assets.
        /// </summary>
        private IAssetManager _assets;

        /// <summary>
        /// Loads and executes scripts.
        /// </summary>
        private IScriptManager _scripts;

        /// <summary>
        /// Manages pooling.
        /// </summary>
        private IAssetPoolManager _pools;

        /// <summary>
        /// True iff Setup has been called.
        /// </summary>
        private bool _setup;

        /// <summary>
        /// Token for asset readiness.
        /// </summary>
        private readonly AsyncToken<Content> _onAssetReady = new AsyncToken<Content>();

        /// <summary>
        /// Token for script readiness.
        /// </summary>
        private readonly AsyncToken<Content> _onScriptsReady = new AsyncToken<Content>();
        
        /// <summary>
        /// The Asset.
        /// </summary>
        private Asset _asset;

        /// <summary>
        /// Instance of Asset's prefab.
        /// </summary>
        private GameObject _instance;

        /// <summary>
        /// An action to unsubscribe from <c>Asset</c> updates.
        /// </summary>
        private Action _unwatch;

        /// <summary>
        /// Content data.
        /// </summary>
        public ContentData Data { get; private set; }
        


        /// <summary>
        /// Called to setup the content.
        /// </summary>
        /// <param name="assets">Loads assets.</param>
        /// <param name="scripts">Loads + executes scripts.</param>
        /// <param name="pools">Manages pooling.</param>
        /// <param name="data">Data to setup with.</param>
        public void Setup(
            IAssetManager assets,
            IScriptManager scripts,
            IAssetPoolManager pools,
            ContentData data)
        {
            if (_setup)
            {
                throw new Exception(string.Format(
                    "Already initialized content. Existing: {0}, Requestd: {1}.",
                    Data.Name,
                    data.Name));
            }
            _setup = true;

            _assets = assets;
            _scripts = scripts;
            _pools = pools;

            UpdateData(data);
        }

        /// <summary>
        /// Destroyes this instance. Should not be called directly, but through
        /// <c>IContentManager</c> Release flow.
        /// </summary>
        public void Destroy()
        {
            _scripts.ReleaseAll(Data.Tags);

            Destroy(gameObject);
        }

        /// <summary>
        /// Updates <c>ContentData</c> for this instance.
        /// </summary>
        public void UpdateData(ContentData data)
        {
            Data = data;

            RefreshAsset();
            RefreshScripts();
        }

        /// <summary>
        /// Tears down the asset and sets it back up.
        /// </summary>
        private void RefreshAsset()
        {
            TeardownAsset();
            SetupAsset();
        }

        private void RefreshScripts()
        {
            TeardownScripts();
            SetupScripts();
        }

        private void SetupAsset()
        {
            // get the corresponding asset
            _asset = Asset(Data);
            if (null == _asset)
            {
                Log.Warning(this,
                    "Could not find Asset for content {0}.",
                    Data);
                
                return;
            }

            _asset.OnRemoved += Asset_OnRemoved;

            // watch for asset reloads
            _unwatch = _asset.Watch<GameObject>(value =>
            {
                Log.Info(this, "Asset loaded.");

                if (null != _instance)
                {
                    _pools.Put(_instance);
                    _instance = null;
                }

                _instance = _pools.Get<GameObject>(value);
                _instance.transform.SetParent(transform, false);

                UpdateInstancePosition();

                _onAssetReady.Succeed(this);
            });

            // automatically reload
            _asset.AutoReload = true;
        }

        /// <summary>
        /// Destroys the asset and stops watching it.
        /// </summary>
        private void TeardownAsset()
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
        }

        /// <summary>
        /// Loads all scripts.
        /// </summary>
        private void SetupScripts()
        {
            var scripts = Data.Scripts;
            var len = scripts.Count;
            if (0 == len)
            {
                _onScriptsReady.Succeed(this);
                return;
            }

            for (var i = 0; i < len; i++)
            {
                var data = scripts[i];

                var script = _scripts.Create(data.ScriptDataId, Data.Tags);
//                script.OnReady
            }
        }

        private void TeardownScripts()
        {
            
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

        private void UpdateInstancePosition()
        {
            // configure
            if (Data.PreserveColor)
            {
                ColorMode = ColorMode.InheritAlpha;
            }

            // parent + orient
            _instance.name = Data.Asset.AssetDataId;
            _instance.transform.SetParent(transform);
            /*
            _instance.transform.localPosition = assetPrefab.transform.localPosition;
            _instance.transform.localRotation = assetPrefab.transform.localRotation;
            _instance.transform.localScale = assetPrefab.transform.localScale;*/
            _instance.SetActive(true);

            LocalVisible = true;
        }

        /// <summary>
        /// Called when the asset has been removed from the manifest.
        /// </summary>
        /// <param name="asset">The asset that has been removed.</param>
        private void Asset_OnRemoved(Asset asset)
        {
            TeardownAsset();
        }
    }
}