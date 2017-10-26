using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
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
        private readonly IContentManager _content;
        private readonly HierarchyFocusManager _focus;

        /// <summary>
        /// Backing variable for Graph property.
        /// </summary>
        private readonly ContentGraph _graph;

        /// <summary>
        /// Lookup from ContentData id to GameObject instance.
        /// </summary>
        private readonly Dictionary<string, Content> _contentMap = new Dictionary<string, Content>();

        /// <summary>
        /// Current selection.
        /// </summary>
        private Content _selection;

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
            IContentManager content,
            IAppDataManager appData,
            HierarchyFocusManager focus,
            ContentGraph graph)
        {
            _content = content;
            _focus = focus;
            _graph = graph;
            
            appData.OnUpdated += AppData_OnUpdated;
        }

        /// <summary>
        /// Creates the GameObject hierarchy from the <c>ContentGraph</c>.
        /// </summary>
        public void Create()
        {
            Create(_graph.Root);

            _graph.Root.OnChildAdded += Graph_OnChildAdded;
            _graph.Root.OnChildRemoved += Graph_OnChildRemoved;
        }

        /// <summary>
        /// Selects a specific piece of content.
        /// </summary>
        /// <param name="contentId">Id of the <c>Content</c> to select.</param>
        public void Select(string contentId)
        {
            Content selection;
            if (_contentMap.TryGetValue(contentId, out selection))
            {
                if (null != _selection)
                {
                    _selection.OnLoaded.Remove(OnSelectionLoaded);
                }

                _selection = selection;
                _selection
                    .OnLoaded
                    .OnSuccess(OnSelectionLoaded);
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

            // check that parent exists first
            Content content;
            if (!_contentMap.TryGetValue(parent.Id, out content))
            {
                Log.Error(this,
                    "Child added to a node HierarchyManager has not created : {0}.",
                    parent.Id);
                return;
            }

            Create(child);
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
            // check that child exists first
            Content content;
            if (!_contentMap.TryGetValue(child.Id, out content))
            {
                Log.Error(this,
                    "Child removed that HierarchyManager has not created : {0}.",
                    child.Id);
                return;
            }

            _contentMap.Remove(child.Id);

            _content.Release(content);
        }

        /// <summary>
        /// Recursive method that creates nodes.
        /// </summary>
        /// <param name="node">Node to create.</param>
        private void Create(ContentGraphNode node)
        {
            if (node.Id == "root")
            {
                // ignore
            }
            else
            {
                var contentId = node.ContentId;
                var content = _content.Request(contentId);
                _contentMap[contentId] = content;
            }

            // children
            var children = node.Children;
            for (int i = 0, len = children.Count; i < len; i++)
            {
                Create(children[i]);
            }
        }
        
        /// <summary>
        /// Called when AppData has an update.
        /// </summary>
        /// <param name="staticData">The StaticData that was updated.</param>
        private void AppData_OnUpdated(StaticData staticData)
        {
            Content content;
            if (_contentMap.TryGetValue(staticData.Id, out content))
            {
                Log.Info(this, "Pushing ContentData update to Content.");

                content.UpdateData((ContentData) staticData);
            }
        }

        /// <summary>
        /// Called when the current selection has been reloaded.
        /// </summary>
        /// <param name="content">Content.</param>
        private void OnSelectionLoaded(Content content)
        {
            _focus.Focus(content.gameObject);
        }
    }
}