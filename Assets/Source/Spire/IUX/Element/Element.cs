using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer.IUX
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
        /// Scratch list for FindAll.
        /// </summary>
        private readonly List<Element> _findAllScratch = new List<Element>();

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
        /// Invoked when element is destroyed.
        /// </summary>
        public event Action<Element> OnDestroyed;

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
        /// Creates an element.
        /// </summary>
        public Element()
        {
            Schema = new ElementSchema("Unknown");
        }
        
        /// <summary>
        /// String override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("<{0} Id={1} Guid={2} />",
                GetType().Name,
                Id,
                Guid);
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
            Element[] children)
        {
            Guid = System.Guid.NewGuid().ToString();
            Id = data.Id;
            Schema = schema;
            Schema.Identifier = data.Id;

            BeforeLoadChildrenInternal();

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

            Log.Info(this, "Load({0})", Guid);

            AfterLoadChildrenInternal();
        }

        /// <summary>
        /// Unloads for reuse.
        /// </summary>
        internal void Unload()
        {
            // unload children first
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                _children[i].Unload();
            }
            _children.Clear();

            Log.Info(this, "Unload({0})", Guid);

            UnloadInternal();

            Id = string.Empty;

            // TODO: create on Load(), NOT constructor/Unload
            Schema = new ElementSchema("Unknown");
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
            // destroy children
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                _children[i].Destroy();
            }

            Log.Info(this, "Destroy({0})", Guid);

            UnloadInternal();
            DestroyInternal();

            if (OnDestroyed != null)
            {
                OnDestroyed(this);
                OnDestroyed = null;
            }
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

            // hook up schema
            element.Schema.Wrap(Schema);

            AddChildInternal(element);

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

                // unwrap schema
                element.Schema.Wrap(null);

                RemoveChildInternal(element);

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
        /// Finds a single element.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <returns></returns>
        public T FindOne<T>(string query) where T : Element
        {
            _findAllScratch.Clear();
            Find(query, _findAllScratch);
            
            return _findAllScratch
                .OfType<T>()
                .FirstOrDefault();
        }

        /// <summary>
        /// Finds a list of elements.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <param name="results">Results passed in.</param>
        public void Find(string query, IList<Element> results)
        {
            Find<Element>(query, results);
        }

        /// <summary>
        /// Finds a list of elements.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <param name="results">Results passed in.</param>
        public void Find<T>(string query, IList<T> results) where T : Element
        {
            if (string.IsNullOrEmpty(query))
            {
                return;
            }

            // split at recursive queries
            var current = new List<Element> { this };
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
                        return;
                    }

                    // execute query on each of the current nodes
                    var matches = new List<Element>();
                    for (int j = 0, jlen = current.Count; j < jlen; j++)
                    {
                        ExecuteQueryRecursive(
                            current[j],
                            elementQuery,
                            matches);
                    }

                    if (0 != matches.Count)
                    {
                        current = matches;
                    }
                    else
                    {
                        results.Clear();
                        return;
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
                        results.Clear();
                        return;
                    }

                    // execute query on each of the current nodes
                    var matches = new List<Element>();
                    for (int l = 0, llen = current.Count; l < llen; l++)
                    {
                        ExecuteQuery(
                            current[l],
                            elementQuery,
                            matches);
                    }

                    if (0 != matches.Count)
                    {
                        current = matches;
                    }
                    else
                    {
                        results.Clear();
                        return;
                    }
                }

                recur = true;
            }
            
            // add
            for (int i = 0, len = current.Count; i < len; i++)
            {
                var cast = current[i] as T;
                if (null != cast)
                {
                    results.Add(cast);
                }
            }
        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void BeforeLoadChildrenInternal()
        {

        }
        
        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void AfterLoadChildrenInternal()
        {
            
        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void UnloadInternal()
        {

        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void DestroyInternal()
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
        /// For base classes to override.
        /// </summary>
        protected virtual void AddChildInternal(Element element)
        {
            
        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void RemoveChildInternal(Element element)
        {

        }

        /// <summary>
        /// Executes a query on an element.
        /// </summary>
        /// <param name="element">Element in question.</param>
        /// <param name="query">The query object.</param>
        /// <param name="results">Results of the query.</param>
        private void ExecuteQuery(Element element, ElementQuery query, List<Element> results)
        {
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
        private void ExecuteQueryRecursive(Element element, ElementQuery query, List<Element> results)
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

        /// <summary>
        /// Recursive append method.
        /// </summary>
        /// <param name="builder">Constructs strings.</param>
        /// <param name="element">The element at which to start.</param>
        /// <param name="tabs">Indentation</param>
        private void Append(
            StringBuilder builder,
            Element element,
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