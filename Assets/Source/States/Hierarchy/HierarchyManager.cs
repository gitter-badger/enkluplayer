using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class HierarchyManager
    {
        private readonly IAssetManager _assets;
        private readonly IAssetPoolManager _pools;
        private readonly IAppDataManager _appData;
        private readonly ContentGraph _graph;
        private readonly GameObject _root;
        private readonly Dictionary<string, HierarchyNodeMonoBehaviour> _gameObjects = new Dictionary<string, HierarchyNodeMonoBehaviour>();

        public ContentGraph Graph
        {
            get { return _graph; }
        }

        public HierarchyManager(
            IAssetManager assets,
            IAssetPoolManager pools,
            IAppDataManager appData,
            ContentGraph graph)
        {
            _assets = assets;
            _pools = pools;
            _appData = appData;
            _graph = graph;

            _root = new GameObject("Hierarchy");
        }

        public void Create()
        {
            foreach (var child in _graph.Root.Children)
            {
                Create(_root.transform, child);
            }
        }

        public void Select(string contentId)
        {
            Log.Error(this, "Select({0}) is not implemented yet.", contentId);
        }

        public void Clear()
        {
            _graph.Clear();

            throw new NotImplementedException();
        }

        private void Create(Transform parent, ContentGraph.ContentGraphNode node)
        {
            var contentData = _appData.Get<ContentData>(node.ContentId);
            if (null == contentData)
            {
                Log.Error(this,
                    "Could not create HierarchyNodeMonoBehaviour for {0} : No content by that id.",
                    node.ContentId);
                return;
            }

            // TODO: pool
            var gameObject = new GameObject(contentData.Name);
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;

            var behavior = gameObject.AddComponent<HierarchyNodeMonoBehaviour>();
            behavior.Initialize(
                _assets,
                _pools,
                contentData);

            _gameObjects[contentData.Id] = behavior;

            // children
            var children = node.Children;
            var transform = gameObject.transform;
            for (int i = 0, len = children.Count; i < len; i++)
            {
                Create(transform, children[i]);
            }
        }
    }
}