using System;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A specific GameObject representation of a Content node.
    /// </summary>
    public class HierarchyNodeMonoBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private IAssetManager _assets;
        private IAssetPoolManager _pools;

        /// <summary>
        /// The data of this node.
        /// </summary>
        private ContentData _data;

        /// <summary>
        /// The <c>Asset</c> this node displays.
        /// </summary>
        private Asset _asset;

        /// <summary>
        /// An action to unsubscribe from <c>Asset</c> updates.
        /// </summary>
        private Action _unwatch;
        private bool _originalAutoLoad;
        private GameObject _instance;

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="assets"></param>
        /// <param name="pools"></param>
        /// <param name="data"></param>
        public void Initialize(
            IAssetManager assets,
            IAssetPoolManager pools,
            ContentData data)
        {
            _assets = assets;
            _pools = pools;
            _data = data;

            SetupAsset();
        }

        public void Uninitialize()
        {
            TeardownAsset();
        }

        public void ContentUpdate(ContentData data)
        {
            _data = data;

            // check for asset changes
            if (null != _asset)
            {
                if (null != data.Asset
                    && data.Asset.AssetDataId == _asset.Data.Guid)
                {
                    return;
                }

                RefreshAsset();
            }
            else if (null != data.Asset
                && !string.IsNullOrEmpty(data.Asset.AssetDataId))
            {
                RefreshAsset();
            }
        }

        private void SetupAsset()
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
            });

            // automatically reload
            _originalAutoLoad = _asset.AutoReload;
            _asset.AutoReload = true;
        }

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
                _asset.AutoReload = _originalAutoLoad;
                _asset = null;
            }
        }

        private void RefreshAsset()
        {
            TeardownAsset();
            SetupAsset();
        }

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

        private void Asset_OnRemoved(Asset asset)
        {
            Uninitialize();
        }
    }
}