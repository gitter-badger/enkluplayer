﻿using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Synchonizes <c>Element</c> system and <c>HierarchyDatabase</c>.
    /// </summary>
    public class HierarchyManager
    {
        /// <summary>
        /// Tags for requesting <c>Content</c>.
        /// </summary>
        private const string CONTENT_TAGS = "hierarchymanager";

        private readonly IContentManager _content;
        private readonly HierarchyDatabase _database;
        private readonly FocusManager _focus;
        
        /// <summary>
        /// Lookup from ContentData id to Content instance.
        /// </summary>
        private readonly Dictionary<string, ContentWidget> _contentMap = new Dictionary<string, ContentWidget>();

        /// <summary>
        /// Current selection.
        /// </summary>
        private ContentWidget _selectedContent;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public HierarchyManager(
            IAppDataManager appData,
            IContentManager content,
            HierarchyDatabase database,
            FocusManager focus)
        {
            _content = content;
            _database = database;
            _focus = focus;

            appData.OnUpdated += AppData_OnUpdated;
        }

        /// <summary>
        /// Creates the GameObject hierarchy from the <c>ContentGraph</c>.
        /// </summary>
        public void Startup()
        {
            Create(_database.Root);

            _database.OnNodeAdded += Hierarchy_OnNodeAdded;
            _database.OnNodeRemoved += Graph_OnChildRemoved;
            _database.OnNodeUpdated += Graph_OnUpdated;
        }

        /// <summary>
        /// Selects a specific piece of content.
        /// </summary>
        /// <param name="contentId">Id of the <c>Content</c> to select.</param>
        public void Select(string contentId)
        {
            ContentWidget selection;
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
                _focus.Focus(_selectedContent.GameObject);

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
        public void Teardown()
        {
            _database.OnNodeUpdated -= Graph_OnUpdated;
            _database.OnNodeRemoved -= Graph_OnChildRemoved;
            _database.OnNodeAdded -= Hierarchy_OnNodeAdded;

            _contentMap.Clear();

            // kill content
            _content.ReleaseAll(CONTENT_TAGS);
        }
        
        /// <summary>
        /// Called when a child is added somewhere beneath the Root.
        /// </summary>
        /// <param name="parent">The parent node.</param>
        /// <param name="child">The child that has been added.</param>
        private void Hierarchy_OnNodeAdded(
            HierarchyNodeData parent,
            HierarchyNodeData child)
        {
            // check that parent exists first
            ContentWidget content;
            if (parent != _database.Root
                && !_contentMap.TryGetValue(parent.Id, out content))
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
        /// <param name="nodeId">The id of the node.</param>
        private void Graph_OnChildRemoved(string nodeId)
        {
            // check that child exists first
            ContentWidget content;
            if (!_contentMap.TryGetValue(nodeId, out content))
            {
                Log.Error(this,
                    "Child removed that HierarchyManager has not created : {0}.",
                    nodeId);
                return;
            }

            _contentMap.Remove(nodeId);
            _content.Release(content);
        }

        /// <summary>
        /// Called when a node has been updated.
        /// </summary>
        /// <param name="node">Node that was updated.</param>
        private void Graph_OnUpdated(HierarchyNodeData node)
        {
            ContentWidget content;
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
        private void Create(HierarchyNodeData node)
        {
            if (node == _database.Root)
            {
                // ignore root
            }
            else
            {
                var contentId = node.ContentId;
                var content = _content.Request(contentId, CONTENT_TAGS);
                _contentMap[contentId] = content;
                
                // locators enforce Self() to be non-null
                // TODO: This should go through the Anchor system.
                //var self = content.Locators.Self();
                //self.OnUpdated += locator => Locator_OnUpdated(locator, content);
            }

            // create children
            var children = node.Children;
            for (int i = 0, len = children.Length; i < len; i++)
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
            ContentWidget content;
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
        private void Locator_OnUpdated(Locator locator, ContentWidget content)
        {
            Log.Info(this, "Locator updatd.");

            content.GameObject.transform.localPosition = locator.Data.Position.ToVector();
            content.GameObject.transform.localRotation = Quaternion.Euler(locator.Data.Rotation.ToVector());
            content.GameObject.transform.localScale = locator.Data.Scale.ToVector();
        }

        /// <summary>
        /// Called when the current selection has been reloaded.
        /// </summary>
        /// <param name="content">Content.</param>
        private void Selection_OnLoaded(ContentWidget content)
        {
            _focus.Focus(content.GameObject);
        }
    }
}