using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using Enklu.Mycelium.Messages;
using Enklu.Mycelium.Messages.Experience;

namespace CreateAR.EnkluPlayer
{
    public class SceneEventHandler
    {
        private readonly IElementManager _elements;
        private readonly IAppSceneManager _scenes;
        private readonly List<Element> _elementHeap = new List<Element>();

        private ElementMap _map;

        private Element _root;
        private string[] _elementLookup;
        private string[] _propLookup;

        public ElementMap Map
        {
            get { return _map; }
            set
            {
                _map = value;

                BuildMapUpdate();
            }
        }
        
        public SceneEventHandler(
            IElementManager elements,
            IAppSceneManager scenes)
        {
            _elements = elements;
            _scenes = scenes;
        }

        public void Initialize()
        {
            if (_scenes.All.Length == 0)
            {
                Log.Error(this, "Tried to initialize SceneEventHandler but scene manager has no scenes!");
                return;
            }

            _root = _scenes.Root(_scenes.All[0]);
        }

        public void Uninitialize()
        {
            _root = null;
            _elementHeap.Clear();
        }

        public string ElementId(ushort hash)
        {
            if (null != _elementLookup && hash < _elementLookup.Length)
            {
                return _elementLookup[hash];
            }

            return string.Empty;
        }

        public ushort ElementHash(string elementId)
        {
            for (int i = 0, len = _elementLookup.Length; i < len; i++)
            {
                var id = _elementLookup[i];
                if (id == elementId)
                {
                    return (ushort) i;
                }
            }

            return 0;
        }

        public string PropName(ushort hash)
        {
            if (null != _propLookup && hash < _propLookup.Length)
            {
                return _propLookup[hash];
            }

            return string.Empty;
        }

        public void OnDiff(SceneDiffEvent obj)
        {
            Log.Debug(this, "Diff event received: {0}", obj.Map);

            Map = obj.Map;
        }

        public void OnUpdated<T>(T evt) where T : UpdateElementEvent
        {
            if (null == _root)
            {
                return;
            }

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

        public void OnMapUpdated(SceneMapUpdateEvent obj)
        {
            Map.Props = Map.Props.Add(obj.PropsAdded);
            Map.Elements = Map.Elements.Add(obj.ElementsAdded);

            Log.Debug(this, "Map updated : {0}", Map);

            BuildMapUpdate();
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
            len = props.Length;

            _propLookup = new string[len + 1];
            for (var i = 0; i < len; i++)
            {
                var record = props[i];
                _propLookup[record.Hash] = record.Value;
            }
        }
    }
}