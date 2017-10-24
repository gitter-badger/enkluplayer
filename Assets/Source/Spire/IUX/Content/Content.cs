using System;
using System.Collections.Generic;
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
        /// True iff Setup has been called.
        /// </summary>
        private bool _setup;

        /// <summary>
        /// Backing variable for property.
        /// </summary>
        private readonly AsyncToken<Content> _onReady = new AsyncToken<Content>();

        /// <summary>
        /// The instanced asset.
        /// </summary>
        private GameObject _asset;
        
        /// <summary>
        /// Content data.
        /// </summary>
        public ContentData Data { get; private set; }
        
        /// <summary>
        /// AsyncToken fired when content is ready.
        /// </summary>
        public IAsyncToken<Content> OnReady
        {
            get
            {
                return _onReady;
            }
        }

        /// <summary>
        /// Called to setup the content.
        /// </summary>
        /// <param name="assets">Loads assets.</param>
        /// <param name="scripts">Loads + executes scripts.</param>
        /// <param name="data">Data to setup with.</param>
        public void Setup(
            IAssetManager assets,
            IScriptManager scripts,
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

            Data = data;

            LoadAsset(Data);
            LoadScripts(Data.Scripts);
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
        /// Loads the asset + resolves the OnReady token.
        /// </summary>
        /// <param name="data"></param>
        private void LoadAsset(ContentData data)
        {
            var info = _assets.Manifest.Data(data.Asset.AssetDataId);
            if (null == info)
            {
                var error = string.Format(
                    "AssetInfoId does not correspond to any known AssetInfo : {0}",
                    data.Asset);
                Log.Error(this, error);

                _onReady.Fail(new Exception(error));
                return;
            }

            var reference = _assets.Manifest.Asset(info.Guid);
            reference
                .Load<GameObject>()
                .OnSuccess(assetPrefab =>
                {
                    // disable before we instantiate, so it's invisible
                    assetPrefab.SetActive(false);
                    _asset = Instantiate(assetPrefab);
                    
                    // configure
                    if (Data.PreserveColor)
                    {
                        ColorMode = ColorMode.InheritAlpha;
                    }

                    // parent + orient
                    _asset.name = Data.Asset.AssetDataId;
                    _asset.transform.SetParent(transform);
                    _asset.transform.localPosition = assetPrefab.transform.localPosition;
                    _asset.transform.localRotation = assetPrefab.transform.localRotation;
                    _asset.transform.localScale = assetPrefab.transform.localScale;
                    _asset.gameObject.SetActive(true);

                    LocalVisible = true;

                    // success!
                    _onReady.Succeed(this);
                })
                .OnFailure(exception =>
                {
                    var error = string.Format("Could not load asset {0}.", reference);
                    Log.Error(this, error);

                    _onReady.Fail(new Exception(error));
                });
        }

        /// <summary>
        /// Loads all scripts.
        /// </summary>
        /// <param name="scripts">Scripts to load.</param>
        private void LoadScripts(List<ScriptReference> scripts)
        {
            for (int i = 0, len = scripts.Count; i < len; i++)
            {
                var data = scripts[i];

                var script = _scripts.Create(data.ScriptDataId, Data.Tags);
                
            }
        }
    }
}