using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
#if NETFX_CORE
using System.Reflection;
#endif
using System.Text;
using Enklu.Data;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Base class for UI elements.
    ///
    /// Flows:
    ///
    /// Load()
    ///     > LoadInternalBeforeChildren
    ///     > Children > AddChild()
    ///     > LoadInternalAfterChildren
    ///
    /// Unload()
    ///     > UnloadInternalBeforeChildren
    ///     > Children > Unload()
    ///     > UnloadInternalAfterChildren
    ///
    /// Destroy()
    ///     > UnloadInternalBeforeChildren
    ///     > Children > Destroy()
    ///     > UnloadInternalAfterChildren
    ///     > DestroyInternal
    ///
    /// </summary>
    public class Element
    {
        /// <summary>
        /// Stack used for FindFast.
        /// </summary>
        private static readonly int[] _NextChildIndices = new int[128];

        /// <summary>
        /// Internal list of children.
        /// </summary>
        private readonly List<Element> _children = new List<Element>();

        /// <summary>
        /// Scratch list for FindAll.
        /// </summary>
        private readonly List<Element> _findAllScratch = new List<Element>();

        /// <summary>
        /// Id prop.
        /// </summary>
        private ElementSchemaProp<string> _idProp;

        /// <summary>
        /// Name prop.
        /// </summary>
        protected ElementSchemaProp<string> _nameProp;
        
        /// <summary>
        /// Visibility prop.
        /// </summary>
        public ElementSchemaProp<bool> LocalVisibleProp { get; private set; }

        /// <summary>
        /// Unique internal id for this element.
        /// </summary>
        public string Guid { get; private set;  }

        /// <summary>
        /// Unique id stored in data for this element.
        /// </summary>
        public string Id { get; private set; }
        
        /// <summary>
        /// Name accessor.
        /// </summary>
        public string Name
        {
            get { return _nameProp.Value; }
            set { _nameProp.Value = value; }
        }

        /// <summary>
        /// State.
        /// </summary>
        public ElementSchema Schema { get; private set; }

        /// <summary>
        /// Copy of children collection.
        /// </summary>
        public ReadOnlyCollection<Element> Children { get; private set; }

        /// <summary>
        /// The element's parent.
        /// </summary>
        public Element Parent { get; private set; }

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
        public event Action<Element, Element> OnDescendentRemoved;

        /// <summary>
        /// Called when a child, at any depth, has been added to the graph.
        /// </summary>
        public event Action<Element, Element> OnDescendentAdded;

        /// <summary>
        /// Called when an immediate child has been removed from the graph.
        /// </summary>
        public event Action<Element, Element> OnChildRemoved;

        /// <summary>
        /// Called when an immediate child has been added to the graph.
        /// </summary>
        public event Action<Element, Element> OnChildAdded;

        /// <summary>
        /// Creates an element.
        /// </summary>
        public Element()
        {
            Children = new ReadOnlyCollection<Element>(_children);
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
            Schema = schema;

            _idProp = schema.GetOwn("id", data.Id ?? Guid);
            _idProp.OnChanged += Id_OnChange;

            Id = Schema.Identifier = _idProp.Value;

            _nameProp = schema.GetOwn("name", string.Empty);
            LocalVisibleProp = schema.GetOwn("visible", true);

            LoadInternalBeforeChildren();

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

            LogVerbose("Load({0})", Guid);

            LoadInternalAfterChildren();
        }

        /// <summary>
        /// Unloads for reuse.
        /// </summary>
        public void Unload()
        {
            UnloadInternalBeforeChildren();

            // unload children first
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                _children[i].Unload();
            }
            _children.Clear();

            LogVerbose("Unload({0})", Guid);

            UnloadInternalAfterChildren();

            _idProp.OnChanged -= Id_OnChange;
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
            if (OnDestroyed != null)
            {
                OnDestroyed(this);
                OnDestroyed = null;
            }

            UnloadInternalBeforeChildren();

            // destroy children
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                child.OnDestroyed -= Child_OnDestroyed;
                child.Destroy();
            }

            LogVerbose("Destroy({0})", Guid);

            UnloadInternalAfterChildren();
            DestroyInternal();
        }

        /// <summary>
        /// Adds an element as a child of this element. If the element is
        /// already a child, this does nothing.
        /// </summary>
        /// <param name="element">Element to add as a child.</param>
        public void AddChild(Element element)
        {
            if (null == element)
            {
                throw new ArgumentNullException("element");
            }

            // trivial case
            if (element.Parent == this)
            {
                return;
            }

            // remove from previous parent
            if (null != element.Parent)
            {
                element.Parent.RemoveChild(element);
            }

            // add to this element's list
            _children.Add(element);
            element.Parent = this;
            element.OnDescendentAdded += Child_OnDescendentAdded;
            element.OnDescendentRemoved += Child_OnDescendentRemoved;
            element.OnDestroyed += Child_OnDestroyed;

            // hook up schema
            element.Schema.Wrap(Schema);

            AddChildInternal(element);

            OnDescendentAdded.Execute(this, element);
            OnChildAdded.Execute(this, element);
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
                element.Parent = null;
                element.OnDescendentAdded -= Child_OnDescendentAdded;
                element.OnDescendentRemoved -= Child_OnDescendentRemoved;
                element.OnDestroyed -= Child_OnDestroyed;

                // unwrap schema
                element.Schema.Wrap(null);

                RemoveChildInternal(element);

                // emit events
                element.OnRemoved.Execute(element);
                OnDescendentRemoved.Execute(this, element);
                OnChildRemoved.Execute(this, element);
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
#if NETFX_CORE
                .Where(el => el.GetType() == typeof(T) || el.GetType().GetTypeInfo().IsSubclassOf(typeof(T)))
#else
                .Where(el => el.GetType() == typeof(T) || el.GetType().IsSubclassOf(typeof(T)))
#endif
                .Cast<T>()
                .FirstOrDefault();
        }

        /// <summary>
        /// Stack-less search through hierarchy for an element that matches by
        /// id.
        /// </summary>
        /// <param name="root">Where to start the search.</param>
        /// <param name="id">The id to look for.</param>
        /// <returns></returns>
        public Element FindFast(Element root, string id)
        {
            var el = root;
            var compare = true;

            // prep indices
            var depthIndex = 0;
            _NextChildIndices[0] = 0;

            // search!
            while (true)
            {
                if (compare && el.Id == id)
                {
                    return el;
                }

                // get the index to the next child at this depth
                var nextChildIndex = _NextChildIndices[depthIndex];

                // proceed to next child
                if (nextChildIndex < el.Children.Count)
                {
                    // increment next child index at this depth
                    _NextChildIndices[depthIndex]++;

                    // get the next child
                    el = el.Children[nextChildIndex];

                    // move to the next depth
                    _NextChildIndices[++depthIndex] = 0;

                    // switch compare back on
                    compare = true;
                }
                // there is no next child
                else
                {
                    // move up a level
                    depthIndex--;

                    // there is nowhere else to go
                    if (depthIndex < 0)
                    {
                        return null;
                    }

                    // parent element
                    el = el.Parent;

                    // don't compare ids, we've already checked this element
                    compare = false;
                }
            }
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
        /// Returns true if this Element is a child or indirect child of the passed in Element.
        /// </summary>
        /// <param name="element">Potential parent to check.</param>
        /// <returns></returns>
        public bool IsChildOf(Element element)
        {
            Element parent = this;
            do
            {
                parent = parent.Parent;
            } while (parent != null && parent != element);

            return parent != null;
        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void LoadInternalBeforeChildren()
        {

        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void LoadInternalAfterChildren()
        {

        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void UnloadInternalBeforeChildren()
        {

        }

        /// <summary>
        /// For base classes to override.
        /// </summary>
        protected virtual void UnloadInternalAfterChildren()
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
            for (int i = 0, len = children.Count; i < len; i++)
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
            for (int i = 0, len = children.Count; i < len; i++)
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
        private void Child_OnDescendentAdded(Element _, Element child)
        {
            if (null != OnDescendentAdded)
            {
                OnDescendentAdded(this, child);
            }
        }

        /// <summary>
        /// Called when a child dispatches an OnChildRemoved event.
        /// </summary>
        /// <param name="_">The parent, which we disregard.</param>
        /// <param name="child">The child.</param>
        private void Child_OnDescendentRemoved(Element _, Element child)
        {
            if (null != OnDescendentRemoved)
            {
                OnDescendentRemoved(this, child);
            }
        }

        /// <summary>
        /// Called when an immediate child has been destroyed.
        /// </summary>
        /// <param name="element">The child element.</param>
        private void Child_OnDestroyed(Element element)
        {
            RemoveChild(element);
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
            for (int i = 0, len = children.Count; i < len; i++)
            {
                Append(builder, children[i], tabs + 1);
            }
        }

        /// <summary>
        /// Called when the id changes.
        /// </summary>
        /// <param name="prop">Prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Id_OnChange(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            Id = next;
        }

        /// <summary>
        /// Verbose logging.
        /// </summary>
        [Conditional("VERBOSE_LOGGING")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}