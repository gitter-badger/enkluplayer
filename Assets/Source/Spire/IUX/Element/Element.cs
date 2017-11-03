using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Base class for UI elements.
    /// </summary>
    public class Element
    {
        /// <summary>
        /// Internal list of children.
        /// </summary>
        private readonly List<Element> _children = new List<Element>();

        /// <summary>
        /// Unique internal id for this element.
        /// </summary>
        public string Guid { get; private set;  }

        /// <summary>
        /// Unique id stored in data for this element.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// State.
        /// </summary>
        public ElementSchema Schema { get; private set; }

        /// <summary>
        /// Copy of children collection.
        /// </summary>
        public Element[] Children
        {
            get
            {
                return _children.ToArray();
            }
        }
        
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
        /// Prepares an element for use.
        /// </summary>
        /// <param name="data">Associated saved data.</param>
        /// <param name="schema">The schema for this element.</param>
        /// <param name="children">Complete set of children.</param>
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

        /// <summary>
        /// Unloads for reuse.
        /// </summary>
        internal void Unload()
        {
            UnloadInternal();

            Id = string.Empty;
            Schema = new ElementSchema();

            _children.Clear();
        }

        /// <summary>
        /// Adds an element as a child of this element. If the element is
        /// already a child, moves it to the end of the list.
        /// </summary>
        /// <param name="element">Element to add as a child.</param>
        public void AddChild(Element element)
        {
            if (null == element)
            {
                throw new ArgumentNullException("element");
            }

            var index = _children.IndexOf(element);
            if (-1 != index)
            {
                element.OnChildAdded -= Child_OnChildAdded;
                element.OnChildRemoved -= Child_OnChildRemoved;
                _children.RemoveAt(index);
            }

            _children.Add(element);

            element.OnChildAdded += Child_OnChildAdded;
            element.OnChildRemoved += Child_OnChildRemoved;

            if (null != OnChildAdded)
            {
                OnChildAdded(this, element);
            }
        }

        /// <summary>
        /// Removes an element as a child.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        /// <returns></returns>
        public bool RemoveChild(Element element)
        {
            if (null == element)
            {
                throw new ArgumentNullException("element");
            }

            var removed = _children.Remove(element);
            if (removed)
            {
                element.OnChildAdded -= Child_OnChildAdded;
                element.OnChildRemoved -= Child_OnChildRemoved;

                if (null != element.OnRemoved)
                {
                    element.OnRemoved(element);
                }

                if (null != OnChildRemoved)
                {
                    OnChildRemoved(this, element);
                }
            }

            return removed;
        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void LoadInternal()
        {
            
        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void UnloadInternal()
        {

        }

        /// <summary>
        /// Called when a child dispatches an OnChildAdded event.
        /// </summary>
        /// <param name="_">The parent, which we disregard.</param>
        /// <param name="child">The child.</param>
        private void Child_OnChildAdded(Element _, Element child)
        {
            if (null != OnChildAdded)
            {
                OnChildAdded(this, child);
            }
        }

        /// <summary>
        /// Called when a child dispatches an OnChildRemoved event.
        /// </summary>
        /// <param name="_">The parent, which we disregard.</param>
        /// <param name="child">The child.</param>
        private void Child_OnChildRemoved(Element _, Element child)
        {
            if (null != OnChildRemoved)
            {
                OnChildRemoved(this, child);
            }
        }
    }
}