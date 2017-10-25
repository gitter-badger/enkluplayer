/*using System;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using ContentGraphNode = CreateAR.SpirePlayer.ContentGraph.ContentGraphNode;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A specific GameObject representation of a Content node.
    /// 
    /// TODO: Just switch to Content?
    /// </summary>
    public class HierarchyNodeMonoBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private IAssetManager _assets;
        private IAssetPoolManager _pools;
        private IScriptManager _scripts;

        /// <summary>
        /// The data of this node.
        /// </summary>
        private ContentData _data;

        /// <summary>
        /// Node in ContentGraph.
        /// </summary>
        private ContentGraphNode _node;

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
        /// Called when the asset has been updated.
        /// </summary>
        public event Action<HierarchyNodeMonoBehaviour> OnAssetUpdated;

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="assets">Loads assets.</param>
        /// <param name="pools">Pools objects.</param>
        /// <param name="scripts">Loads scripts.</param>
        /// <param name="data">Data for this piece of content.</param>
        /// <param name="node">Node in ContentGraph for this content.</param>
        public void Initialize(
            IAssetManager assets,
            IAssetPoolManager pools,
            IScriptManager scripts,
            ContentData data,
            ContentGraphNode node)
        {
            _assets = assets;
            _pools = pools;
            _data = data;
            _node = node;

            _node.OnUpdated += Node_OnUpdate;

            SetupAsset();
        }

        public void Uninitialize()
        {
            _node.OnUpdated -= Node_OnUpdate;

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

                UpdateInstancePosition();

                if (null != OnAssetUpdated)
                {
                    OnAssetUpdated(this);
                }
            });

            // automatically reload
            _originalAutoLoad = _asset.AutoReload;
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
                _asset.AutoReload = _originalAutoLoad;
                _asset = null;
            }
        }

        /// <summary>
        /// Tears down the asset and sets it back up.
        /// </summary>
        private void RefreshAsset()
        {
            TeardownAsset();
            SetupAsset();
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
        /// Updates the instance's position based on the self locator.
        /// </summary>
        private void UpdateInstancePosition()
        {
            if (null == _instance)
            {
                return;
            }

            var selfLocator = _node.SelfLocator;
            if (null == selfLocator)
            {
                Log.Warning(this, "Node {0} has no self locator.", _node);
            }
            else
            {
                _instance.transform.localPosition = selfLocator.Position;
                _instance.transform.localRotation = Quaternion.Euler(selfLocator.Rotation);
                _instance.transform.localScale = selfLocator.Scale;
            }
        }

        /// <summary>
        /// Called when the asset has been removed from the manifest.
        /// </summary>
        /// <param name="asset">The asset that has been removed.</param>
        private void Asset_OnRemoved(Asset asset)
        {
            Uninitialize();
        }

        /// <summary>
        /// Called when the node is updated.
        /// </summary>
        /// <param name="node">The node.</param>
        private void Node_OnUpdate(ContentGraphNode node)
        {
            // position
            UpdateInstancePosition();

            // scripts

        }
    }
}*/