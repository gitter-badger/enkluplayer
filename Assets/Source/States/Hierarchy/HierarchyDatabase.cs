using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Object that stores hierarchy information separate from the <c>Element</c>
    /// system.
    /// </summary>
    public class HierarchyDatabase
    {
        /// <summary>
        /// List of top level nodes.
        /// </summary>
        private readonly List<HierarchyNodeData> _nodes = new List<HierarchyNodeData>();

        /// <summary>
        /// Readonly collection that wraps <c>_nodes</c> field.
        /// </summary>
        private readonly ReadOnlyCollection<HierarchyNodeData> _publicNodes;

        /// <summary>
        /// Called when a node has been added.
        /// </summary>
        public event Action<string, HierarchyNodeData> OnNodeAdded;

        /// <summary>
        /// Called when a node has been removed.
        /// </summary>
        public event Action<string> OnNodeRemoved;

        /// <summary>
        /// Called when a node has been updated.
        /// </summary>
        public event Action<HierarchyNodeData> OnNodeUpdated;

        /// <summary>
        /// All top level nodes.
        /// </summary>
        public ReadOnlyCollection<HierarchyNodeData> Nodes
        {
            get { return _publicNodes; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HierarchyDatabase()
        {
            _publicNodes = new ReadOnlyCollection<HierarchyNodeData>(_nodes);
        }

        /// <summary>
        /// Sets all top level nodes.
        /// </summary>
        /// <param name="nodes">The collectyion of top level nodes.</param>
        public void Set(HierarchyNodeData[] nodes)
        {
            _nodes.Clear();
            _nodes.AddRange(nodes);
        }

        /// <summary>
        /// Adds a node to a parent.
        /// </summary>
        /// <param name="parentId">The id of the parent.</param>
        /// <param name="node">The node.</param>
        public void Add(string parentId, HierarchyNodeData node)
        {
            HierarchyNodeData match, parent;
            if (!Node(parentId,  out match, out parent))
            {
                Log.Error(this,
                    "Attempted to add {0} to parent {1} but parent does not exist.",
                    node,
                    parentId);

                return;
            }

            if (null != parent)
            {
                for (int i = 0, len = parent.Children.Length; i < len; i++)
                {
                    var child = parent.Children[i];
                    if (child.Id == node.Id)
                    {
                        Log.Error(this,
                            "Attempted to add {0} to parent {1} but child already exists! You probably meant to use Update instead.",
                            node,
                            parent);

                        return;
                    }
                }

                parent.Children = parent.Children.Add(node);
            }

            if (null != OnNodeAdded)
            {
                OnNodeAdded(parentId, node);
            }
        }

        /// <summary>
        /// Updates a node.
        /// </summary>
        /// <param name="node">The new node data.</param>
        public void Update(HierarchyNodeData node)
        {
            HierarchyNodeData match, parent;
            if (!Node(node.Id, out match, out parent))
            {
                Log.Error(this,
                    "Attempted to update {0} but node not found.",
                    node);
                return;
            }

            var updated = false;

            if (null == parent)
            {
                for (int i = 0, len = _nodes.Count; i < len; i++)
                {
                    var child = _nodes[i];
                    if (child.Id == node.Id)
                    {
                        _nodes[i] = node;

                        updated = true;

                        break;
                    }
                }
            }
            else
            {
                for (int i = 0, len = parent.Children.Length; i < len; i++)
                {
                    var child = parent.Children[i];
                    if (child.Id == node.Id)
                    {
                        parent.Children[i] = node;

                        updated = true;

                        break;
                    }
                }
            }

            if (!updated)
            {
                Log.Error(this,
                    "Attempted to update {0}. Found the existing node but could not update the parent.",
                    node);

                return;
            }

            if (null != OnNodeUpdated)
            {
                OnNodeUpdated(node);
            }
        }

        /// <summary>
        /// Removes a node by id.
        /// </summary>
        /// <param name="id">Id of the node.</param>
        public void Remove(string id)
        {
            HierarchyNodeData match, parent;
            if (!Node(id, out match, out parent))
            {
                Log.Error(this,
                    "Attempted to remove {0} but node not found.",
                    id);
                return;
            }

            var removed = false;

            if (null == parent)
            {
                for (int i = 0, len = _nodes.Count; i < len; i++)
                {
                    var child = _nodes[i];
                    if (child.Id == id)
                    {
                        _nodes.RemoveAt(i);

                        removed = true;

                        break;
                    }
                }
            }
            else
            {
                for (int i = 0, len = parent.Children.Length; i < len; i++)
                {
                    var child = parent.Children[i];
                    if (child.Id == id)
                    {
                        parent.Children = parent.Children.Remove(child);

                        removed = true;

                        break;
                    }
                }
            }

            if (!removed)
            {
                Log.Error(this,
                    "Attempted to remove {0}. Found the existing node but could not remove from parent.",
                    id);

                return;
            }

            if (null != OnNodeRemoved)
            {
                OnNodeRemoved(id);
            }
        }

        /// <summary>
        /// Retrieves a node.
        /// </summary>
        /// <param name="id">Unique id of the node.</param>
        /// <param name="match">The matching node.</param>
        /// <param name="parent">The parent of the matching node.</param>
        /// <returns></returns>
        private bool Node(
            string id,
            out HierarchyNodeData match,
            out HierarchyNodeData parent)
        {
            for (int i = 0, len = _nodes.Count; i < len; i++)
            {
                var child = _nodes[i];
                if (child.Id == id)
                {
                    match = child;
                    parent = null;

                    return true;
                }

                if (Node(id, child, out match, out parent))
                {
                    return true;
                }
            }

            parent = match = null;
            return false;
        }

        /// <summary>
        /// Recursive method that retrieves a node.
        /// </summary>
        /// <param name="id">Id of the node.</param>
        /// <param name="start">Node at which to start.</param>
        /// <param name="match">Matching node.</param>
        /// <param name="parent">Parent of the matching node.</param>
        /// <returns></returns>
        private bool Node(
            string id,
            HierarchyNodeData start,
            out HierarchyNodeData match,
            out HierarchyNodeData parent)
        {
            var children = start.Children;
            for (int i = 0, len = children.Length; i < len; i++)
            {
                var child = children[i];
                if (child.Id == id)
                {
                    match = child;
                    parent = start;

                    return true;
                }

                if (Node(id, child, out match, out parent))
                {
                    return true;
                }
            }

            match = parent = null;
            return false;
        }
    }
}