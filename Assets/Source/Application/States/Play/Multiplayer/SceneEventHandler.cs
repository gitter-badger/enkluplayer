using System;
using System.Collections.Generic;
using System.Diagnostics;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using Enklu.Mycelium.Messages;
using Enklu.Mycelium.Messages.Experience;
using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Object that handles events for a scene.
    /// </summary>
    public class SceneEventHandler
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IElementManager _elements;
        private readonly IElementFactory _elementFactory;
        private readonly ScenePatcher _scenePatcher;

        /// <summary>
        /// A simple list of recently used elements.
        /// </summary>
        private readonly List<Element> _elementHeap = new List<Element>();

        /// <summary>
        /// Root element of the scene.
        /// </summary>
        private Element _root;

        /// <summary>
        /// Maps
        /// </summary>
        private ElementMap _map;

        /// <summary>
        /// Lookup from hash to element id.
        /// </summary>
        private string[] _elementLookup;

        /// <summary>
        /// Lookup from hash to prop name.
        /// </summary>
        private string[] _propLookup;

        /// <summary>
        /// Gets/sets the map.
        /// </summary>
        public ElementMap Map
        {
            get
            {
                if (null == _map)
                {
                    _map = new ElementMap();
                }

                return _map;
            }
            set
            {
                _map = value;

                BuildMapUpdate();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneEventHandler(
            IElementManager elements,
            IElementFactory elementFactory,
            ScenePatcher scenePatcher)
        {
            _elements = elements;
            _elementFactory = elementFactory;
            _scenePatcher = scenePatcher;
        }

        /// <summary>
        /// Preps handler for usage.
        /// </summary>
        public void Initialize()
        {
            Log.Info(this, "SceneEventHandler initialized.");

            _scenePatcher.Initialize();
            _elementHeap.Clear();

            // create a default value
            _map = new ElementMap();
        }

        /// <summary>
        /// Tears down any resources in use.
        /// </summary>
        public void Uninitialize()
        {
            Log.Info(this, "SceneEventHandler uninitialized.");

            _elementHeap.Clear();
        }

        /// <summary>
        /// Retrieves the element id from the hash.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <returns></returns>
        public string ElementId(ushort hash)
        {
            if (null != _elementLookup && hash < _elementLookup.Length)
            {
                return _elementLookup[hash];
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves a prop name from hash.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <returns></returns>
        public string PropName(ushort hash)
        {
            if (null != _propLookup && hash < _propLookup.Length)
            {
                return _propLookup[hash];
            }

            return string.Empty;
        }
        
        /// <summary>
        /// Applies a diff to the scene.
        /// </summary>
        /// <param name="evt">The diff event.</param>
        public void OnDiff(SceneDiffEvent evt)
        {
            Log.Warning(this, "Diff event received: {0}", evt.Map);

            Map = evt.Map;
            _scenePatcher.Apply(Expand(evt.ToActions()));
        }
        
        /// <summary>
        /// Processes a <c>CreateElementEvent</c>.
        /// </summary>
        /// <param name="evt">The event.</param>
        public Element OnCreated(CreateElementEvent evt)
        {
            Verbose("OnCreated({0})", JsonConvert.SerializeObject(evt));

            // find parent
            var parentId = ElementId(evt.ParentHash);
            var parent = ById(parentId);
            if (null == parent)
            {
                Log.Warning(this, "Could not find parent to create element under: {0}.", evt.ParentHash);
                return null;
            }

            Element element;
            try
            {
                element = _elementFactory.Element(new ElementDescription
                {
                    Elements = new[] {evt.Element},
                    Root = new ElementRef
                    {
                        Id = evt.Element.Id
                    }
                });
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not create element: {0}", exception);

                return null;
            }

            parent.AddChild(element);
            return element;
        }

        /// <summary>
        /// Processes a <c>DeleteElementEvent</c>.
        /// </summary>
        /// <param name="evt">The event.</param>
        public void OnDeleted(DeleteElementEvent evt)
        {
            Verbose("OnDeleted({0})", JsonConvert.SerializeObject(evt));

            var elementId = ElementId(evt.ElementHash);
            var element = ById(elementId);
            if (null == element)
            {
                Log.Warning(this, "Could not find element to delete: {0}.", evt.ElementHash);
                return;
            }

            element.Destroy();
        }

        /// <summary>
        /// Processes an UpdateElementEvent.
        /// </summary>
        /// <typeparam name="T">The type of event.</typeparam>
        /// <param name="evt">The event.</param>
        public void OnUpdated<T>(T evt) where T : UpdateElementEvent
        {
            Verbose("OnUpdated({0})", JsonConvert.SerializeObject(evt));

            // find element
            var elId = ElementId(evt.ElementHash);
            var el = ById(elId);
            if (null == el)
            {
                Log.Warning(this, "Could not find element to update: {0}.", evt.ElementHash);
                return;
            }

            // prop name
            var propName = PropName(evt.PropHash);
            if (string.IsNullOrEmpty(propName))
            {
                Log.Warning(this, "Could not find prop name from id {0}.", propName);
                return;
            }

            var vec3 = evt as UpdateElementVec3Event;
            if (null != vec3)
            {
                el.Schema.Get<Vec3>(propName).Value = vec3.Value;
                return;
            }

            var fl = evt as UpdateElementFloatEvent;
            if (null != fl)
            {
                el.Schema.Get<float>(propName).Value = fl.Value;
                return;
            }

            var col4 = evt as UpdateElementCol4Event;
            if (null != col4)
            {
                el.Schema.Get<Col4>(propName).Value = col4.Value;
                return;
            }

            var it = evt as UpdateElementIntEvent;
            if (null != it)
            {
                el.Schema.Get<int>(propName).Value = it.Value;
                return;
            }

            var str = evt as UpdateElementStringEvent;
            if (null != str)
            {
                el.Schema.Get<string>(propName).Value = str.Value;
                return;
            }

            var bl = evt as UpdateElementBoolEvent;
            if (null != bl)
            {
                el.Schema.Get<bool>(propName).Value = bl.Value;
                return;
            }

            Log.Error(this, "Could not handle UpdateElementEvent {0}.", evt);
        }

        /// <summary>
        /// Called when the scene map has been updated.
        /// </summary>
        /// <param name="evt">The update event.</param>
        public void OnMapUpdated(SceneMapUpdateEvent evt)
        {
            Log.Debug(this, "Map updated: Previous map : {0}", Map);

            Map.Props = Map.Props.Add(evt.PropsAdded);
            Map.Elements = Map.Elements.Add(evt.ElementsAdded);

            Log.Debug(this, "Map updated: Next map : {0}", Map);

            BuildMapUpdate();
        }

        /// <summary>
        /// Applies the hashing to the elementId and key.
        /// </summary>
        private List<ElementActionData> Expand(List<ElementActionData> actions)
        {
            for (int i = 0; i < actions.Count; ++i)
            {
                var action = actions[i];
                action.ElementId = ElementId(action.ElementHash);
                action.Key = PropName(action.KeyHash);
            }

            return actions;
        }

        /// <summary>
        /// Retrieves an element by id and stores it in a local data structure
        /// for fast lookup.
        /// </summary>
        /// <param name="id">The element id.</param>
        /// <returns></returns>
        private Element ById(string id)
        {
            Element el;

            for (int i = 0, len = _elementHeap.Count; i < len; i++)
            {
                el = _elementHeap[i];
                if (el.Id == id)
                {
                    return el;
                }
            }

            el = _elements.ById(id);

            if (null != el)
            {
                _elementHeap.Add(el);
            }

            return el;
        }

        /// <summary>
        /// Builds out fast data structures for map lookups.
        /// </summary>
        private void BuildMapUpdate()
        {
            // populate element lookup
            var elements = _map.Elements;
            var len = elements.Length;

            _elementLookup = new string[len + 1];
            for (var i = 0; i < len; i++)
            {
                var record = elements[i];
                _elementLookup[record.Hash] = record.Value;
            }

            //  populate prop lookup
            var props = _map.Props;
            var max = 0;
            for (var i = 0; i < props.Length; i++)
            {
                var value = props[i].Hash;
                if (value > max)
                {
                    max = value;
                }
            }

            len = props.Length;
            _propLookup = new string[max + 1];
            for (var i = 0; i < len; i++)
            {
                var record = props[i];
                _propLookup[record.Hash] = record.Value;
            }
        }

        /// <summary>
        /// Verbose logging.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string format, params object[] replacements)
        {
            Log.Info(this, format, replacements);
        }
    }
}