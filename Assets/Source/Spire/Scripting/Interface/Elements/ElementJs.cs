using System;
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
        private readonly Dictionary<string, List<Func<JsValue, JsValue[], JsValue>>> _events = new Dictionary<string, List<Func<JsValue, JsValue[], JsValue>>>();
        
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
        /// Runs scripts.
        /// </summary>
        protected readonly IScriptManager _scripts;

        /// <summary>
        /// Caches ElementJs instances for an engine.
        /// </summary>
        protected readonly IElementJsCache _cache;

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
        public void on(string eventType, Func<JsValue, JsValue[], JsValue> fn)
        {
            EventList(eventType).Add(fn);
        }

        /// <inheritdoc />
        public void off(string eventType)
        {
            EventList(eventType).Clear();
        }

        /// <inheritdoc />
        public void off(string eventType, Func<JsValue, JsValue[], JsValue> fn)
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

            Log.Info(this, "Dispatch '{0}' event to {1} listeners.", eventType, count);

            var param = new[] { JsValue.FromObject(_engine, evt) };
            if (1 == count)
            {
                list[0](_this, param);
            }
            else
            {
                var copy = list.ToArray();
                for (var i = 0; i < count; i++)
                {
                    copy[i](_this, param);
                }
            }
        }

        /// <summary>
        /// Retrieves the list of event handlers for an event type.
        /// </summary>
        /// <param name="eventType">The type.</param>
        /// <returns></returns>
        private List<Func<JsValue, JsValue[], JsValue>> EventList(string eventType)
        {
            List<Func<JsValue, JsValue[], JsValue>> list;
            if (!_events.TryGetValue(eventType, out list))
            {
                list = _events[eventType] = new List<Func<JsValue, JsValue[], JsValue>>();
            }

            return list;
        }
    }
}