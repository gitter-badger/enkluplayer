using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Base class for UI elements.
    /// </summary>
    public class Element : IElement
    {
        /// <summary>
        /// Internal list of children.
        /// </summary>
        private readonly List<IElement> _children = new List<IElement>();

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
        public IElement[] Children
        {
            get
            {
                return _children.ToArray();
            }
        }

        /// <summary>
        /// Invoked when element is destroyed.
        /// </summary>
        public event Action<IElement> OnDestroyed;

        /// <summary>
        /// Called when this node has been removed from the graph.
        /// </summary>
        public event Action<IElement> OnRemoved;

        /// <summary>
        /// Called when a child, at any depth, has been removed from the graph.
        /// </summary>
        public event Action<IElement, IElement> OnChildRemoved;

        /// <summary>
        /// Called when a child, at any depth, has been added to the graph.
        /// </summary>
        public event Action<IElement, IElement> OnChildAdded;

        /// <summary>
        /// Creates an element.
        /// </summary>
        public Element()
        {
            Schema = new ElementSchema();
        }

        /// <summary>
        /// Useful ToString;
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[Element Id={0}, Schema={1}]",
                Id,
                Schema);
        }

        /// <summary>
        /// Outputs a string of the entire graph.
        /// </summary>
        /// <returns></returns>
        public string ToTreeString()
        {
            var builder = new StringBuilder();

            Append(builder, this);

            return builder.ToString();
        }

        /// <summary>
        /// Prepares an element for use.
        /// </summary>
        /// <param name="data">Associated saved data.</param>
        /// <param name="schema">The schema for this element.</param>
        /// <param name="children">Complete set of children.</param>
        public void Load(
            ElementData data,
            ElementSchema schema,
            IElement[] children)
        {
            Guid = System.Guid.NewGuid().ToString();
            Id = data.Id;
            Schema = schema;

            // child schemas wrap parent
            for (int i = 0, len = children.Length; i < len; i++)
            {
                var child = children[i];
                if (child != null)
                {
                    child.Schema.Wrap(Schema);
                    AddChild(child);
                }
            }

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
        /// Frame based update.
        /// </summary>
        public void FrameUpdate()
        {
            UpdateInternal();
        }

        /// <summary>
        /// Frame based update.
        /// </summary>
        public void LateFrameUpdate()
        {
            LateUpdateInternal();
        }

        /// <summary>
        /// Destroys the widget
        /// </summary>
        public void Destroy()
        {
            if (OnDestroyed != null)
            {
                Unload();

                if (OnDestroyed != null)
                {
                    OnDestroyed(this);
                    OnDestroyed = null;
                }
            }
        }

        /// <summary>
        /// Adds an element as a child of this element. If the element is
        /// already a child, moves it to the end of the list.
        /// </summary>
        /// <param name="element">Element to add as a child.</param>
        public void AddChild(IElement element)
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

            // hook up schema
            element.Schema.Wrap(Schema);

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
        public bool RemoveChild(IElement element)
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

                // unwrap schema
                element.Schema.Wrap(null);

                var Element = element as Element;
                if (null != Element.OnRemoved)
                {
                    Element.OnRemoved(element);
                }

                if (null != OnChildRemoved)
                {
                    OnChildRemoved(this, element);
                }
            }

            return removed;
        }

        /// <summary>
        /// Finds a single element.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <returns></returns>
        public IElement FindOne(string query)
        {
            return Find(query).FirstOrDefault();
        }

        /// <summary>
        /// Queries for a set of elements.
        /// </summary>
        /// <param name="query">The query in question.</param>
        public List<IElement> Find(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new List<IElement>();
            }

            // split at recursive queries
            var current = new List<IElement>{ this };
            var recur = false;
            if (query.StartsWith(".."))
            {
                recur = true;

                query = query.Substring(2);
            }

            var recursiveQueries = query.Split(
                new[] { ".." },
                StringSplitOptions.None).ToList();
            
            for (int i = 0, len = recursiveQueries.Count; i < len; i++)
            {
                var recursiveQuery = recursiveQueries[i];
                
                // split into shallow queries
                var shallowQueries = recursiveQuery.Split('.').ToList();

                // recursive queries
                if (recur)
                {
                    var recursiveQueryString = shallowQueries[0];
                    shallowQueries.RemoveAt(0);

                    // create query
                    var elementQuery = new ElementQuery(recursiveQueryString);
                    if (!elementQuery.IsValid)
                    {
                        return new List<IElement>();
                    }

                    // execute query on each of the current nodes
                    var results = new List<IElement>();
                    for (int j = 0, jlen = current.Count; j < jlen; j++)
                    {
                        ExecuteQueryRecursive(
                            current[j],
                            elementQuery,
                            results);
                    }

                    if (0 != results.Count)
                    {
                        current = results;
                    }
                    else
                    {
                        return new List<IElement>();
                    }
                }

                // perform shallow searches
                for (int k = 0, klen = shallowQueries.Count; k < klen; k++)
                {
                    var shallowQueryString = shallowQueries[k];

                    // create query
                    var elementQuery = new ElementQuery(shallowQueryString);
                    if (!elementQuery.IsValid)
                    {
                        return new List<IElement>();
                    }

                    // execute query on each of the current nodes
                    var results = new List<IElement>();
                    for (int l = 0, llen = current.Count; l < llen; l++)
                    {
                        ExecuteQuery(
                            current[l],
                            elementQuery,
                            results);
                    }

                    if (0 != results.Count)
                    {
                        current = results;
                    }
                    else
                    {
                        return new List<IElement>();
                    }
                }

                recur = true;
            }

            return current;
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
        /// Invoked once per frame.
        /// </summary>
        protected virtual void UpdateInternal()
        {
            
        }

        /// <summary>
        /// Invoked once per frame.
        /// </summary>
        protected virtual void LateUpdateInternal()
        {

        }

        /// <summary>
        /// Executes a query on an element.
        /// </summary>
        /// <param name="element">Element in question.</param>
        /// <param name="query">The query object.</param>
        /// <param name="results">Results of the query.</param>
        private void ExecuteQuery(IElement element, ElementQuery query, List<IElement> results)
        {
            // test self
            // TODO: THIS DOESN'T SEEM RIGHT, called from hierarchy of shallow queries seperated by "."
            // WOULD EXPECT BASED ON USAGE THAT THIS SHOULD ONLY BE CHECKING CHILDREN
            //if (query.Execute(element))
            //{
            //    results.Add(element);
            //}

            // test children
            var children = element.Children;
            for (int i = 0, len = children.Length; i < len; i++)
            {
                var child = children[i];
                if (query.Execute(child))
                {
                    results.Add(child);
                }
            }
        }

        /// <summary>
        /// Executes a query recursively.
        /// </summary>
        /// <param name="element">The element to search.</param>
        /// <param name="query">Query object.</param>
        /// <param name="results">Result list.</param>
        private void ExecuteQueryRecursive(IElement element, ElementQuery query, List<IElement> results)
        {
            var children = element.Children;
            for (int i = 0, len = children.Length; i < len; i++)
            {
                var child = children[i];
                if (query.Execute(child))
                {
                    results.Add(child);
                }

                ExecuteQueryRecursive(child, query, results);
            }
        }

        /// <summary>
        /// Called when a child dispatches an OnChildAdded event.
        /// </summary>
        /// <param name="_">The parent, which we disregard.</param>
        /// <param name="child">The child.</param>
        private void Child_OnChildAdded(IElement _, IElement child)
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
        private void Child_OnChildRemoved(IElement _, IElement child)
        {
            if (null != OnChildRemoved)
            {
                OnChildRemoved(this, child);
            }
        }

        /// <summary>
        /// Recursive append method.
        /// </summary>
        /// <param name="builder">Constructs strings.</param>
        /// <param name="element">The element at which to start.</param>
        /// <param name="tabs">Indentation</param>
        private void Append(
            StringBuilder builder,
            IElement element,
            int tabs = 0)
        {
            for (var i = 0; i < tabs; i++)
            {
                builder.Append("\t");
            }

            builder.AppendFormat("{0}\n", element);

            var children = element.Children;
            for (int i = 0, len = children.Length; i < len; i++)
            {
                Append(builder, children[i], tabs + 1);
            }
        }
    }
}