using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

using ContentGraphNode = CreateAR.SpirePlayer.ContentGraph.ContentGraphNode;

namespace CreateAR.SpirePlayer
{
    public class HierarchyManager
    {
        private readonly IAssetManager _assets;
        private readonly IAssetPoolManager _pools;
        private readonly IAppDataManager _appData;
        private readonly ContentGraph _graph;
        private readonly GameObject _root;
        private readonly Dictionary<string, GameObject> _gameObjects = new Dictionary<string, GameObject>();

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
            Create(_root.transform, _graph.Root);

            _graph.Root.OnChildAdded += Graph_OnChildAdded;
            _graph.Root.OnChildRemoved += Graph_OnChildRemoved;
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

        private void Graph_OnChildAdded(
            ContentGraphNode root,
            ContentGraphNode child)
        {
            var parent = child.Parent;

            GameObject gameObject;
            if (!_gameObjects.TryGetValue(parent.Id, out gameObject))
            {
                Log.Error(this,
                    "Child added to a node HierarchyManager has not created : {0}.",
                    parent.Id);
                return;
            }

            Create(gameObject.transform, child);
        }

        private void Graph_OnChildRemoved(
            ContentGraphNode root,
            ContentGraphNode child)
        {
            GameObject gameObject;
            if (!_gameObjects.TryGetValue(child.Id, out gameObject))
            {
                Log.Error(this,
                    "Child removed that HierarchyManager has not created : {0}.",
                    child.Id);
                return;
            }

            _gameObjects.Remove(child.Id);

            UnityEngine.Object.Destroy(gameObject);
        }

        private void Create(Transform parent, ContentGraphNode node)
        {
            Transform transform;
            if (node.Id == "root")
            {
                var root = _gameObjects[node.Id] = new GameObject("Root");

                transform = root.transform;
            }
            else
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
                    new StaticDataWatcher<ContentData>(_appData, contentData));

                _gameObjects[contentData.Id] = gameObject;

                transform = gameObject.transform;
            }

            // children
            var children = node.Children;
            for (int i = 0, len = children.Count; i < len; i++)
            {
                Create(transform, children[i]);
            }
        }
    }
}