using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A composite data structure with content as each node.
    /// </summary>
    public class ContentGraph
    {
        /// <summary>
        /// Graph node within the graph.
        /// </summary>
        public class ContentGraphNode
        {
            /// <summary>
            /// Id unique to the graph.
            /// </summary>
            public string Id { get; private set; }

            /// <summary>
            /// Id of the content to reference.
            /// </summary>
            public string ContentId { get; private set; }

            /// <summary>
            /// Child nodes.
            /// </summary>
            public List<ContentGraphNode> Children { get; private set; }

            /// <summary>
            /// Called when this node has been updated. Not called for child
            /// add/removes.
            /// </summary>
            public event Action<ContentGraphNode> OnUpdated;

            /// <summary>
            /// Called when this node has been removed from the graph.
            /// </summary>
            public event Action<ContentGraphNode> OnRemoved;

            /// <summary>
            /// Called when a child, at any depth, has been removed from the graph.
            /// </summary>
            public event Action<ContentGraphNode, ContentGraphNode> OnChildRemoved;

            /// <summary>
            /// Called when a child, at any depth, has been added to the graph.
            /// </summary>
            public event Action<ContentGraphNode, ContentGraphNode> OnChildAdded;

            /// <summary>
            /// Called when a child, at any depth, has been updated.
            /// </summary>
            public event Action<ContentGraphNode, ContentGraphNode> OnChildUpdated;

            /// <summary>
            /// Useful ToString.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("[ContentGraphNode Id={0}]", Id);
            }

            /// <summary>
            /// Finds a node by unique id.
            /// </summary>
            /// <param name="id">The id of the node in question.</param>
            /// <returns></returns>
            public ContentGraphNode FindOne(string id)
            {
                return FindOne(id, this);
            }

            /// <summary>
            /// Same as FindOne, but only searches immediate children.
            /// </summary>
            /// <param name="id">The unique id of the node.</param>
            /// <returns></returns>
            public ContentGraphNode Child(string id)
            {
                for (int i = 0, len = Children.Count; i < len; i++)
                {
                    var child = Children[i];
                    if (child.Id == id)
                    {
                        return child;
                    }
                }

                return null;
            }

            /// <summary>
            /// Calls the OnUpdated event.
            /// </summary>
            internal void Updated()
            {
                if (null != OnUpdated)
                {
                    OnUpdated(this);
                }
            }

            /// <summary>
            /// Calls the OnRemoved event.
            /// </summary>
            internal void Removed()
            {
                if (null != OnRemoved)
                {
                    OnRemoved(this);
                }
            }

            /// <summary>
            /// Creates a new node.
            /// </summary>
            /// <param name="id">Unique id of this node.</param>
            /// <param name="contentId">Unique id of the Content to reference.</param>
            /// <param name="children">All child nodes.</param>
            internal ContentGraphNode(
                string id,
                string contentId,
                ContentGraphNode[] children)
            {
                Id = id;
                ContentId = contentId;
                Children = new List<ContentGraphNode>(children);

                for (int i = 0, len = Children.Count; i < len; i++)
                {
                    AddPropagationHandlers(Children[i]);
                }
            }

            /// <summary>
            /// Clears children, but does not fire events.
            /// </summary>
            internal void ClearChildren()
            {
                Children.Clear();
            }

            /// <summary>
            /// Adds a child node.
            /// </summary>
            /// <param name="node">The node to add.</param>
            internal void AddChild(ContentGraphNode node)
            {
                AddPropagationHandlers(node);

                Children.Add(node);
                
                // call events on self
                if (null != OnChildAdded)
                {
                    OnChildAdded(this, node);
                }
            }

            /// <summary>
            /// Removes a child node.
            /// </summary>
            /// <param name="node">Thenode to remove.</param>
            internal void RemoveChild(ContentGraphNode node)
            {
                Children.Remove(node);
                
                // call event on node
                if (null != node.OnRemoved)
                {
                    node.OnRemoved(node);
                }
            }

            /// <summary>
            /// Adds handlers to proagate events up the graph.
            /// </summary>
            /// <param name="node">The node that has been added.</param>
            private void AddPropagationHandlers(ContentGraphNode node)
            {
                node.OnChildAdded += (_, child) =>
                {
                    if (null != OnChildAdded)
                    {
                        OnChildAdded(this, child);
                    }
                };

                node.OnChildRemoved += (_, child) =>
                {
                    if (null != OnChildRemoved)
                    {
                        OnChildRemoved(this, child);
                    }
                };

                node.OnChildUpdated += (_, child) =>
                {
                    if (null != OnChildUpdated)
                    {
                        OnChildUpdated(this, child);
                    }
                };

                node.OnUpdated += _ =>
                {
                    if (null != OnChildUpdated)
                    {
                        OnChildUpdated(this, node);
                    }
                };

                node.OnRemoved += _ =>
                {
                    if (null != OnChildRemoved)
                    {
                        OnChildRemoved(this, node);
                    }
                };
            }

            /// <summary>
            /// Recursive search method.
            /// </summary>
            /// <param name="id">Unique id of the node.</param>
            /// <param name="node">The node to start the search from.</param>
            /// <returns></returns>
            private ContentGraphNode FindOne(string id, ContentGraphNode node)
            {
                if (id == node.Id)
                {
                    return node;
                }

                var children = node.Children;
                for (int i = 0, len = children.Count; i < len; i++)
                {
                    var result = FindOne(id, children[i]);
                    if (null != result)
                    {
                        return result;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// The root node.
        /// </summary>
        public ContentGraphNode Root { get; private set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentGraph()
        {
            Root = new ContentGraphNode("root", "root", new ContentGraphNode[0]);
        }
        
        /// <summary>
        /// Clears the root node without firing any events.
        /// </summary>
        public void Clear()
        {
            Root.ClearChildren();
        }

        /// <summary>
        /// Retrieves a node by unique id.
        /// </summary>
        /// <param name="id">Unique id of the node.</param>
        /// <returns></returns>
        public ContentGraphNode FindOne(string id)
        {
            return FindOne(id, Root);
        }

        /// <summary>
        /// Adds a node to the graph.
        /// </summary>
        /// <param name="parentId">Id of the parent node.</param>
        /// <param name="data">Data to construct more of the graph.</param>
        /// <returns></returns>
        public bool Add(string parentId, params HierarchyNodeData[] data)
        {
            var parent = FindOne(parentId);
            if (null == parent)
            {
                return false;
            }

            foreach (var child in data)
            {
                parent.AddChild(Create(child));
            }

            return true;
        }

        /// <summary>
        /// Removes a node.
        /// </summary>
        /// <param name="id">Id of the node to remove.</param>
        /// <returns></returns>
        public bool Remove(string id)
        {
            ContentGraphNode child;

            // trivial case
            if (Root.Id == id)
            {
                child = Root;

                Root = null;

                child.Removed();

                return true;
            }
            
            // find node's parent
            var parent = FindParent(id, Root, out child);
            if (null == parent)
            {
                return false;
            }

            parent.RemoveChild(child);

            return true;
        }

        /// <summary>
        /// Updates a portion of the graph.
        /// </summary>
        /// <param name="data">The data to update.</param>
        /// <returns></returns>
        public bool Update(HierarchyNodeData data)
        {
            var node = FindOne(data.Id);
            if (null == node)
            {
                return false;
            }

            Reconcile(data, node);

            return true;
        }

        /// <summary>
        /// Reconciles changes in data with a node.
        /// </summary>
        /// <param name="data">The authoritative data.</param>
        /// <param name="node">The node needing updated.</param>
        private void Reconcile(HierarchyNodeData data, ContentGraphNode node)
        {
            // add or update children
            var dataChildren = data.Children;
            for (int i = 0, len = dataChildren.Length; i < len; i++)
            {
                var childData = dataChildren[i];
                var child = node.Child(childData.Id);
                if (null != child)
                {
                    Reconcile(childData, child);
                }
                else
                {
                    node.AddChild(Create(childData));
                }
            }

            // remove children
            var nodeChildren = node.Children;
            for (var i = nodeChildren.Count - 1; i >= 0; i--)
            {
                var childNode = nodeChildren[i];

                // find data entry
                var found = false;
                for (int j = 0, jlen = dataChildren.Length; j < jlen; j++)
                {
                    var childData = dataChildren[j];
                    if (childData.Id == childNode.Id)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    // remove child
                    node.RemoveChild(childNode);
                }
            }

            // fire event on node
            node.Updated();
        }

        /// <summary>
        /// Creates a node, recusively.
        /// </summary>
        /// <param name="data">The data to create a node from.</param>
        /// <returns></returns>
        private ContentGraphNode Create(HierarchyNodeData data)
        {
            // build out children first
            var children = data.Children;
            var childNodes = new ContentGraphNode[children.Length];
            for (int i = 0, len = children.Length; i < len; i++)
            {
                childNodes[i] = Create(children[i]);
            }

            // build out graph node
            return new ContentGraphNode(
                data.Id,
                data.ContentId,
                childNodes);
        }

        /// <summary>
        /// Recursive query method using a depth first search.
        /// </summary>
        /// <param name="id">Unique id of the node to find.</param>
        /// <param name="start">Start position.</param>
        /// <returns></returns>
        private ContentGraphNode FindOne(string id, ContentGraphNode start)
        {
            if (start.Id == id)
            {
                return start;
            }

            var children = start.Children;
            for (int i = 0, len = children.Count; i < len; i++)
            {
                var node = FindOne(id, children[i]);
                if (null != node)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursive method to find the parent of a particular node.
        /// </summary>
        /// <param name="id">Id of the child.</param>
        /// <param name="start">Start node for the search.</param>
        /// <param name="child">Returns the child.</param>
        /// <returns></returns>
        private ContentGraphNode FindParent(
            string id,
            ContentGraphNode start,
            out ContentGraphNode child)
        {
            // search all children first
            var children = start.Children;
            for (int i = 0, len = children.Count; i < len; i++)
            {
                if (children[i].Id == id)
                {
                    child = children[i];
                    return start;
                }
            }

            // recurse
            for (int i = 0, len = children.Count; i < len; i++)
            {
                var match = FindParent(id, children[i], out child);
                if (null != match)
                {
                    return match;
                }
            }

            child = null;
            return null;
        }
    }
}