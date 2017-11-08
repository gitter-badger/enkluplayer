using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public interface IContentAssembler
    {
        event Action OnSetupComplete;
        event Action OnTeardownComplete;

        void Initialize(Content content);

        void Setup(ContentData data);
        void Teardown();
    }

    public class ModelContentAssembler : IContentAssembler
    {
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
        /// Content.
        /// </summary>
        private Content _content;

        /// <summary>
        /// Data.
        /// </summary>
        private ContentData _data;

        public event Action OnSetupComplete;
        public event Action OnTeardownComplete;

        public ModelContentAssembler(
            IAssetManager assets,
            IAssetPoolManager pools)
        {
            _assets = assets;
            _pools = pools;
        }

        public void Initialize(Content content)
        {
            _content = content;
        }

        public void Setup(ContentData data)
        {
            _data = data;

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
                ReplaceInstance(prefab);
            }

            // watch for asset reloads
            // TODO: No way to see if asset load failed!
            _unwatch = _asset.Watch<GameObject>(value =>
            {
                Log.Info(this, "Asset loaded.");

                ReplaceInstance(value);

                if (null != OnSetupComplete)
                {
                    OnSetupComplete();
                }
            });

            // automatically reload
            _asset.AutoReload = true;
        }

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
        }

        /// <summary>
        /// Creates an instance of the loaded asset and replaces the existing
        /// instance, if there is one.
        /// </summary>
        /// <param name="value">The GameObject that was loaded.</param>
        private void ReplaceInstance(GameObject value)
        {
            // put existing instance back
            if (null != _instance)
            {
                _pools.Put(_instance);
                _instance = null;
            }

            // get a new one
            _instance = _pools.Get<GameObject>(value);
            _instance.transform.SetParent(
                _content.transform,
                false);

            UpdateInstancePosition();
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
        /// Updates the position of the asset instance.
        /// </summary>
        private void UpdateInstancePosition()
        {
            // parent + orient
            _instance.name = _data.Asset.AssetDataId;
            _instance.transform.SetParent(_content.transform);
            /*
            _instance.transform.localPosition = assetPrefab.transform.localPosition;
            _instance.transform.localRotation = assetPrefab.transform.localRotation;
            _instance.transform.localScale = assetPrefab.transform.localScale;*/
            _instance.SetActive(true);
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