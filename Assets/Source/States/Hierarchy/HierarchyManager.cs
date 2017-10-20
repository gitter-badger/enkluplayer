using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

using ContentGraphNode = CreateAR.SpirePlayer.ContentGraph.ContentGraphNode;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the mapping between Unity objects and the ContentGraph.
    /// </summary>
    public class HierarchyManager
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IAssetManager _assets;
        private readonly IAssetPoolManager _pools;
        private readonly IAppDataManager _appData;
        private readonly HierarchyFocusManager _focus;

        /// <summary>
        /// Backing variable for Graph property.
        /// </summary>
        private readonly ContentGraph _graph;

        /// <summary>
        /// GameObject to attach everything to.
        /// </summary>
        private readonly GameObject _root;

        /// <summary>
        /// Lookup from ContentData id to GameObject.
        /// </summary>
        private readonly Dictionary<string, GameObject> _gameObjects = new Dictionary<string, GameObject>();
        
        /// <summary>
        /// Holds relationships between Content.
        /// </summary>
        public ContentGraph Graph
        {
            get { return _graph; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HierarchyManager(
            IAssetManager assets,
            IAssetPoolManager pools,
            IAppDataManager appData,
            HierarchyFocusManager focus,
            ContentGraph graph)
        {
            _assets = assets;
            _pools = pools;
            _appData = appData;
            _focus = focus;
            _graph = graph;

            _root = new GameObject("Hierarchy");

            _appData.OnUpdated += AppData_OnUpdated;
        }

        /// <summary>
        /// Creates the GameObject hierarchy from the <c>ContentGraph</c>.
        /// </summary>
        public void Create()
        {
            Create(_root.transform, _graph.Root);

            _graph.Root.OnChildAdded += Graph_OnChildAdded;
            _graph.Root.OnChildRemoved += Graph_OnChildRemoved;
        }

        /// <summary>
        /// Selects a specific piece of content.
        /// </summary>
        /// <param name="contentId">Id of the <c>Content</c> to select.</param>
        public void Select(string contentId)
        {
            GameObject selection;
            if (_gameObjects.TryGetValue(contentId, out selection))
            {
                _focus.Focus(selection.GetComponent<HierarchyNodeMonoBehaviour>());
            }
        }

        /// <summary>
        /// Clears the GameObjects.
        /// </summary>
        public void Clear()
        {
            _graph.Clear();

            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Called when a child is added somewhere beneath the Root.
        /// </summary>
        /// <param name="root">The root node.</param>
        /// <param name="child">The child that has been added.</param>
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

        /// <summary>
        /// Called when a child has been removed from the graph.
        /// </summary>
        /// <param name="root">The root node.</param>
        /// <param name="child">The child node.</param>
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

        /// <summary>
        /// Recursive method that creates nodes.
        /// </summary>
        /// <param name="parent">Parent to attach to.</param>
        /// <param name="node">Node to create.</param>
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
                    contentData);

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
        
        /// <summary>
        /// Called when AppData has an update.
        /// </summary>
        /// <param name="staticData">The StaticData that was updated.</param>
        private void AppData_OnUpdated(StaticData staticData)
        {
            GameObject gameObject;
            if (_gameObjects.TryGetValue(staticData.Id, out gameObject))
            {
                gameObject
                    .GetComponent<HierarchyNodeMonoBehaviour>()
                    .ContentUpdate((ContentData)staticData);
            }
        }
    }
}