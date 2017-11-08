using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public interface IContentAssembler
    {
        event Action<GameObject> OnSetupComplete;
        
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
        /// Data.
        /// </summary>
        private ContentData _data;

        /// <summary>
        /// Called when asset is setup.
        /// </summary>
        public event Action<GameObject> OnSetupComplete;
        
        public ModelContentAssembler(
            IAssetManager assets,
            IAssetPoolManager pools)
        {
            _assets = assets;
            _pools = pools;
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

            // asset is loaded
            if (null != OnSetupComplete)
            {
                OnSetupComplete(_instance);
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
        /// Called when the asset has been removed from the manifest.
        /// </summary>
        /// <param name="asset">The asset that has been removed.</param>
        private void Asset_OnRemoved(Asset asset)
        {
            Teardown();
        }
    }
}