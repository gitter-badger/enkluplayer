using System;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class HierarchyNodeMonoBehaviour : MonoBehaviour
    {
        private IAssetManager _assets;
        private IAssetPoolManager _pools;
        private StaticDataWatcher<ContentData> _data;

        private Asset _asset;
        private Action _unwatch;
        private bool _originalAutoLoad;
        private GameObject _instance;

        public void Initialize(
            IAssetManager assets,
            IAssetPoolManager pools,
            StaticDataWatcher<ContentData> data)
        {
            _assets = assets;
            _pools = pools;
            _data = data;

            PrepAsset();

            // watch for content update
            _data.OnUpdated += ContentData_OnUpdated;
        }

        public void Uninitialize()
        {
            UnprepAsset();

            _data.Destroy();
        }

        private void PrepAsset()
        {
            // get the corresponding asset
            _asset = Asset(_data.Value);
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

        private void UnprepAsset()
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
            UnprepAsset();
            PrepAsset();
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

        private void ContentData_OnUpdated(ContentData contentData)
        {
            // check for asset changes
            if (null != _asset)
            {
                if (null != contentData.Asset
                    && contentData.Asset.AssetDataId == _asset.Data.Guid)
                {
                    return;
                }

                RefreshAsset();
            }
            else if (null != contentData.Asset && !string.IsNullOrEmpty(contentData.Asset.AssetDataId))
            {
                RefreshAsset();
            }
        }
    }
}