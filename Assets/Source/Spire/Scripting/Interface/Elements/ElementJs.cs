using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint;
using Jint.Native;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Js API for an element.
    /// </summary>
    public class ElementJs : IJsEventDispatcher
    {
        /// <summary>
        /// Used to track events.
        /// </summary>
        private readonly Dictionary<string, List<ICallable>> _events = new Dictionary<string, List<ICallable>>();

        /// <summary>
        /// Runs scripts.
        /// </summary>
        private readonly IScriptManager _scripts;

        /// <summary>
        /// Caches ElementJs instances for an engine.
        /// </summary>
        private readonly IElementJsCache _cache;
        
        /// <summary>
        /// Scratch list for find.
        /// </summary>
        private readonly List<Element> _findScratch = new List<Element>();

        /// <summary>
        /// The JS engine.
        /// </summary>
        private readonly Engine _engine;

        /// <summary>
        /// This value.
        /// </summary>
        private readonly JsValue _this;

        /// <summary>
        /// Element we're wrapping.
        /// </summary>
        protected readonly Element _element;

        /// <summary>
        /// The schema interface.
        /// </summary>
        public readonly ElementSchemaJsApi schema;
        
        /// <summary>
        /// The transform interface.
        /// </summary>
        public readonly ElementTransformJsApi transform;

        /// <summary>
        /// Unique id of the element.
        /// </summary>
        public string id
        {
            get { return _element.Id; }
        }

        /// <summary>
        /// Type name of the element.
        /// </summary>
        public string type
        {
            get { return _element.GetType().Name; }
        }
        
        /// <summary>
        /// Array of children.
        /// </summary>
        public ElementJs[] children
        {
            get
            {
                var all = _element.Children;

                var wrappers = new ElementJs[all.Count];
                for (int i = 0, len = all.Count; i < len; i++)
                {
                    wrappers[i] = _cache.Element(all[i]);
                }

                return wrappers;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementJs(
            IScriptManager scripts,
            IElementJsCache cache, 
            Engine engine,
            Element element)
        {
            _scripts = scripts;
            _element = element;
            _cache = cache;
            _engine = engine;
            
            schema = new ElementSchemaJsApi(engine, _element.Schema);
            transform = new ElementTransformJsApi(_element);

            _this = JsValue.FromObject(_engine, this);
        }
        
        /// <summary>
        /// Adds a child.
        /// </summary>
        /// <param name="element">The element to add as a child.</param>
        public void addChild(ElementJs element)
        {
            if (null == element)
            {
                return;
            }

            _element.AddChild(element._element);
        }

        /// <summary>
        /// Removes a child.
        /// </summary>
        /// <param name="element">The child to remove.</param>
        public bool removeChild(ElementJs element)
        {
            if (null == element)
            {
                return false;
            }

            return _element.RemoveChild(element._element);
        }
        
        /// <summary>
        /// Finds a single element by a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public ElementJs findOne(string query)
        {
            var element = _element.FindOne<Element>(query);

            Log.Info(this, "Query : {0} - {1}", _element, element);

            return _cache.Element(element);
        }

        /// <summary>
        /// Finds a collection of elements that match a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public ElementJs[] find(string query)
        {
            _findScratch.Clear();
            _element.Find(query, _findScratch);

            var results = new ElementJs[_findScratch.Count];
            for (int i = 0, len = _findScratch.Count; i < len; i++)
            {
                results[i] = _cache.Element(_findScratch[i]);
            }

            return results;
        }

        /// <summary>
        /// Destroys element.
        /// </summary>
        public virtual void destroy()
        {
            _element.Destroy();
        }

        /// <summary>
        /// Passes a message to all attached scripts.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parameters">The parameters.</param>
        public void send(string name, params object[] parameters)
        {
            _scripts.Send(_element.Id, name, parameters);
        }

        /// <inheritdoc />
        public void on(string eventType, ICallable fn)
        {
            EventList(eventType).Add(fn);
        }

        /// <inheritdoc />
        public void off(string eventType)
        {
            EventList(eventType).Clear();
        }

        /// <inheritdoc />
        public void off(string eventType, ICallable fn)
        {
            EventList(eventType).Remove(fn);
        }

        /// <summary>
        /// Dispatches an event.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="evt">The event.</param>
        protected void Dispatch(string eventType, object evt)
        {
            var list = EventList(eventType);
            var count = list.Count;
            if (0 == count)
            {
                return;
            }

            var param = new[] { JsValue.FromObject(_engine, evt) };
            if (1 == count)
            {
                list[0].Call(_this, param);
            }
            else
            {
                var copy = list.ToArray();
                for (var i = 0; i < count; i++)
                {
                    copy[i].Call(_this, param);
                }
            }
        }

        /// <summary>
        /// Retrieves the list of event handlers for an event type.
        /// </summary>
        /// <param name="eventType">The type.</param>
        /// <returns></returns>
        private List<ICallable> EventList(string eventType)
        {
            List<ICallable> list;
            if (!_events.TryGetValue(eventType, out list))
            {
                list = _events[eventType] = new List<ICallable>();
            }

            return list;
        }
    }
}