using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.UI
{
    public class Element
    {
        private readonly List<Element> _children = new List<Element>();

        public string Guid { get; private set;  }
        public string Id { get; private set; }
        public ElementSchema Schema { get; private set; }

        public Element[] Children
        {
            get
            {
                return _children.ToArray();
            }
        }

        /// <summary>
        /// Called when this node has been updated. Not called for child
        /// add/removes.
        /// </summary>
        public event Action<Element> OnUpdated;

        /// <summary>
        /// Called when this node has been removed from the graph.
        /// </summary>
        public event Action<Element> OnRemoved;

        /// <summary>
        /// Called when a child, at any depth, has been removed from the graph.
        /// </summary>
        public event Action<Element, Element> OnChildRemoved;

        /// <summary>
        /// Called when a child, at any depth, has been added to the graph.
        /// </summary>
        public event Action<Element, Element> OnChildAdded;

        /// <summary>
        /// Called when a child, at any depth, has been updated.
        /// </summary>
        public event Action<Element, Element> OnChildUpdated;

        internal void Load(
            ElementData data,
            ElementSchema schema,
            Element[] children)
        {
            Guid = System.Guid.NewGuid().ToString();
            Id = data.Id;
            Schema = schema;

            _children.AddRange(children);

            LoadInternal();
        }

        internal void Unload()
        {
            UnloadInternal();

            Id = string.Empty;

            _children.Clear();
        }

        public void AddChild(Element child)
        {
            var index = _children.IndexOf(child);
            if (-1 != index)
            {
                _children.RemoveAt(index);
            }

            _children.Add(child);

            if (null != OnChildAdded)
            {
                OnChildAdded(this, child);
            }
        }

        public bool RemoveChild(Element child)
        {
            var removed = _children.Remove(child);
            if (removed)
            {
                if (null != OnChildRemoved)
                {
                    OnChildRemoved(this, child);
                }
            }

            return removed;
        }

        protected virtual void LoadInternal()
        {
            
        }

        protected virtual void UnloadInternal()
        {

        }
    }
}