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
        /// Tags for requesting <c>Content</c>.
        /// </summary>
        private const string CONTENT_TAGS = "hierarchymanager";

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IContentManager _content;
        private readonly FocusManager _focus;
        private readonly ApplicationConfig _config;

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
        private Content _selectedContent;

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
            ApplicationConfig config,
            FocusManager focus,
            ContentGraph graph)
        {
            _content = content;
            _focus = focus;
            _graph = graph;
            _config = config;
            
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
            _graph.Root.OnChildUpdated += Graph_OnUpdated;
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
                // stop listening to the old one (which starts as null)
                if (null != _selectedContent)
                {
                    _selectedContent.OnLoaded.Remove(Selection_OnLoaded);
                }

                // listen to the new one
                _selectedContent = selection;

                // select now
                _focus.Focus(_selectedContent.gameObject);

                // listen for future selections
                _selectedContent
                    .OnLoaded
                    .OnSuccess(Selection_OnLoaded);
            }
            else
            {
                Log.Error(this,
                    "Request to select Content that HierarchyManager has not created : {0}.",
                    contentId);
            }
        }

        /// <summary>
        /// Clears the GameObjects.
        /// </summary>
        public void Clear()
        {
            _graph.Root.OnChildAdded -= Graph_OnChildAdded;
            _graph.Root.OnChildRemoved -= Graph_OnChildRemoved;
            _graph.Root.OnChildUpdated -= Graph_OnUpdated;
            
            _contentMap.Clear();

            // kill content
            _content.ReleaseAll(CONTENT_TAGS);
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
        /// Called when a node has been updated.
        /// </summary>
        /// <param name="root">Root node.</param>
        /// <param name="node">Node that was updated.</param>
        private void Graph_OnUpdated(ContentGraphNode root, ContentGraphNode node)
        {
            Content content;
            if (!_contentMap.TryGetValue(node.Id, out content))
            {
                Log.Error(this,
                    "Node updated that HierarchyManager has not created : {0}.",
                    node.Id);
                return;
            }

            Log.Info(this, "Hierarchy node updated.");
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
                var content = _content.Request(contentId, CONTENT_TAGS);
                _contentMap[contentId] = content;

                // locators enforce Self() to be non-null
                // TODO: This should go through the Anchor system.
                var self = node.Locators.Self();
                self.OnUpdated += locator => Locator_OnUpdated(locator, content);
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
        /// Called when a locator is updated.
        /// </summary>
        /// <param name="locator">Locator in question.</param>
        /// <param name="content">Content associated with this locator.</param>
        private void Locator_OnUpdated(Locator locator, Content content)
        {
            Log.Info(this, "Locator updatd.");

            content.transform.localPosition = locator.Data.Position.ToVector();
            content.transform.localRotation = Quaternion.Euler(locator.Data.Rotation.ToVector());
            content.transform.localScale = locator.Data.Scale.ToVector();
        }

        /// <summary>
        /// Called when the current selection has been reloaded.
        /// </summary>
        /// <param name="content">Content.</param>
        private void Selection_OnLoaded(Content content)
        {
            _focus.Focus(content.gameObject);
        }
    }
}