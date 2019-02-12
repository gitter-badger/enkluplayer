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
        
        public void OnUpdated<T>(T evt) where T : UpdateElementEvent
        {
            if (null == _root)
            {
                return;
            }

            // find element
            var elId = ElementId(evt.ElementId);
            var el = ById(elId);
            if (null == el)
            {
                Log.Warning(this, "Could not find element to update: {0}.", evt.ElementId);
                return;
            }

            // prop name
            var propName = PropName(evt.PropName);
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

        private readonly int[] _nextChildIndices = new int[128];

        /// <summary>
        /// Stack-less search through hierarchy for an element that matches by
        /// id.
        /// </summary>
        /// <param name="root">Where to start the search.</param>
        /// <param name="id">The id to look for.</param>
        /// <returns></returns>
        private Element FindFast(Element root, string id)
        {
            var el = root;
            var compare = true;

            // prep indices
            var depthIndex = 0;
            _nextChildIndices[0] = 0;

            // search!
            while (true)
            {
                if (compare && el.Id == id)
                {
                    return el;
                }
                
                // get the index to the next child at this depth
                var nextChildIndex = _nextChildIndices[depthIndex];

                // proceed to next child
                if (nextChildIndex < el.Children.Count)
                {
                    // increment next child index at this depth
                    _nextChildIndices[depthIndex]++;

                    // get the next child
                    el = el.Children[nextChildIndex];

                    // move to the next depth
                    _nextChildIndices[++depthIndex] = 0;

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

        private string ElementId(ushort hash)
        {
            if (null != _elementLookup && hash < _elementLookup.Length)
            {
                return _elementLookup[hash];
            }

            return string.Empty;
        }

        private string PropName(ushort hash)
        {
            if (null != _propLookup && hash < _propLookup.Length)
            {
                return _propLookup[hash];
            }

            return string.Empty;
        }
    }
}