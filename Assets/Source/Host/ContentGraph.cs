using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    public class ContentGraph
    {
        public class ContentGraphNode
        {
            public string Id { get; private set; }
            public string ContentId { get; private set; }
            public List<ContentGraphNode> Children { get; private set; }

            public event Action<ContentGraphNode> OnUpdate;
            public event Action<ContentGraphNode> OnRemove;

            public ContentGraphNode FindOne(string id)
            {
                return FindOne(id, this);
            }

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

            internal ContentGraphNode(
                string id,
                string contentId,
                ContentGraphNode[] children)
            {
                Id = id;
                ContentId = contentId;
                Children = new List<ContentGraphNode>(children);
            }

            internal void AddChild(ContentGraphNode node)
            {
                Children.Add(node);

                if (null != OnUpdate)
                {
                    OnUpdate(this);
                }
            }

            internal void RemoveChild(ContentGraphNode node)
            {
                Children.Remove(node);

                if (null != OnUpdate)
                {
                    OnUpdate(this);
                }
            }

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

        public ContentGraphNode Root { get; private set; }

        public event Action<ContentGraphNode> OnLoaded;

        public void Load(HierarchyNodeData data)
        {
            if (null == data)
            {
                throw new Exception("ContentGraph provided with null data.");
            }

            Root = Create(data);

            if (null != OnLoaded)
            {
                OnLoaded(Root);
            }
        }

        public void Clear()
        {
            Root = null;
        }

        public ContentGraphNode FindOne(string id)
        {
            return FindOne(id, Root);
        }

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
            for (int i = 0, ilen = nodeChildren.Count; i < ilen; i++)
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
        }

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
    }
}