using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Manages detecting and dispatching proximity events for Elements. Elements can subscribe and unsubscribe 
    ///     for events against specific Elements. Proximity radii (inner/outer) is set through the Element's schema.
    ///     
    /// Elements subscribed for events are considered "listening". Proximity events are sent when "trigger" Elements
    ///     interact with "listening" Elements. ProximityManager will listen to objects for changes to their schema
    ///     to determine if something should be considered a trigger or not.
    /// </summary>
    [JsInterface("proximity")]
    public class ProximityManager : InjectableMonoBehaviour
    {
        /// <summary>
        /// Fired when a trigger enters a subscribed element's inner radius.
        /// </summary>
        public const string EVENT_ENTER = "enter";

        /// <summary>
        /// Fired every frame after a trigger enters an element, but hasn't exited yet.
        /// </summary>
        public const string EVENT_STAY = "stay";

        /// <summary>
        /// Fired when a trigger exits a subscribed element's outer radius.
        /// </summary>
        public const string EVENT_EXIT = "exit";

        /// <summary>
        /// Schema key for an element's inner radius for proximity checking.
        /// </summary>
        public const string PROP_PROXIMITY_INNER = "proximity.innerRadius";

        /// <summary>
        /// Schema key for an element's outer radius for proximity checking.
        /// </summary>
        public const string PROP_PROXIMITY_OUTER = "proximity.outerRadius";

        /// <summary>
        /// Schema key for an element's status as a trigger for proximity checking.
        /// </summary>
        public const string PROP_PROXIMITY_TRIGGER = "proximity.trigger";

        /// <summary>
        /// Current <see cref="IElementManager"/>, used to track when new Elements are added to watch their schema.
        /// </summary>
        [Inject]
        public IElementManager ElementManager { get; set; }

        /// <summary>
        /// Reference to the <see cref="PlayerJs"/> instance, to act as an always-on trigger.
        /// </summary>
        [Inject]
        public PlayerJs Player { get; set; }

        /// <summary>
        /// The underlying <see cref="ProximityChecker"/> that does the heavy lifting.
        /// </summary>
        private readonly ProximityChecker _proximityChecker = new ProximityChecker();

        /// <summary>
        /// Used just to satisfy ElementJs' ctor. Should be removed when there's only 1 Engine
        /// </summary>
        private readonly Engine _engine = new Engine();

        /// <summary>
        /// Mapping of subscribed ElementJs->callbacks to be invoked when objects enter proximity.
        /// </summary>
        private readonly Dictionary<IEntityJs, List<Func<JsValue, JsValue[], JsValue>>> _enterCallbacks = new Dictionary<IEntityJs, List<Func<JsValue, JsValue[], JsValue>>>();

        /// <summary>
        /// Mapping of subscribed ElementJs->callbacks to be invoked every Update while objects are within proximity.
        /// </summary>
        private readonly Dictionary<IEntityJs, List<Func<JsValue, JsValue[], JsValue>>> _stayCallbacks = new Dictionary<IEntityJs, List<Func<JsValue, JsValue[], JsValue>>>();

        /// <summary>
        /// Mapping of subscribed ElementJs->callbacks to be invoked when objects exit proximity.
        /// </summary>
        private readonly Dictionary<IEntityJs, List<Func<JsValue, JsValue[], JsValue>>> _exitCallbacks = new Dictionary<IEntityJs, List<Func<JsValue, JsValue[], JsValue>>>();

        /// <summary>
        /// Called by Unity. Sets up against existing Elements and watches for new ones.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _proximityChecker.OnEnter += (listener, trigger) => {
                InvokeCallbacks(EVENT_ENTER, listener, trigger);
            };

            _proximityChecker.OnStay += (listener, trigger) => {
                InvokeCallbacks(EVENT_STAY, listener, trigger);
            };

            _proximityChecker.OnExit += (listener, trigger) => {
                InvokeCallbacks(EVENT_EXIT, listener, trigger);
            };

            _proximityChecker.SetElementState(Player, false, true);
            _proximityChecker.SetElementRadii(Player, 1, 1);

            ElementManager.OnCreated += WatchElement;
            foreach (Element element in ElementManager.All)
            {
                WatchElement(element);
            }
        }

        /// <summary>
        /// Called from JsApi. Subscribes an Element to listen for a specified event caused from any trigger.
        /// </summary>
        /// <param name="jsValue">Element that will listen for events</param>
        /// <param name="eventName">Event that will be listened for.</param>
        /// <param name="callback">JS callback to invoke on proximity changes</param>
        public void subscribe(JsValue jsValue, string eventName, Func<JsValue, JsValue[], JsValue> callback)
        {
            var element = ConvertJsValue(jsValue);
            var map = GetMap(eventName);

            List<Func<JsValue, JsValue[], JsValue>> list;
            if (!map.TryGetValue(element, out list))
            {
                list = map[element] = new List<Func<JsValue, JsValue[], JsValue>>();
            }
            list.Add(callback);

            UpdateElement(element);
        }

        /// <summary>
        /// Called from JsApi. Unsubscribes an Element from listening for a specified event caused from any trigger.
        /// </summary>
        /// <param name="jsValue">Element that will be unsubscribed</param>
        /// <param name="eventName"><see cref="ProximityEvents"/> event to unsubscribe from.</param>
        /// <param name="callback">JS callback to unsubscribe</param>
        public void unsubscribe(JsValue jsValue, string eventName, Func<JsValue, JsValue[], JsValue> callback)
        {
            var element = ConvertJsValue(jsValue);

            switch (eventName)
            {
                case EVENT_ENTER:
                {
                    _enterCallbacks.Remove(element);
                    break;
                }
                case EVENT_STAY:
                {
                    _stayCallbacks.Remove(element);
                    break;
                }
                case EVENT_EXIT:
                {
                    _exitCallbacks.Remove(element);
                    break;
                }
                default:
                {
                    Log.Error(this, "Attempted to unsubscribe from an unknown event: " + eventName);
                    return;
                }
            }
            
            UpdateElement(element);
        }

        /// <summary>
        /// Called from Unity. Nudges ProximityChecker to do its job.
        /// </summary>
        private void Update()
        {
            _proximityChecker.Update();
        }

        /// <summary>
        /// Cleans up this resource.
        /// </summary>
        private void OnDestroy()
        {
            _proximityChecker.TearDown();

            _enterCallbacks.Clear();
            _stayCallbacks.Clear();
            _exitCallbacks.Clear();
        }

        /// <summary>
        /// Retrieves the correct map for an event name.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <returns></returns>
        private Dictionary<IEntityJs, List<Func<JsValue, JsValue[], JsValue>>> GetMap(string eventName)
        {
            switch (eventName)
            {
                case EVENT_ENTER:
                {
                    return _enterCallbacks;
                }
                case EVENT_STAY:
                {
                    return _stayCallbacks;
                }
                case EVENT_EXIT:
                {
                    return _exitCallbacks;
                }
                default:
                {
                    Log.Error(this, "Attempted to subscribe to unknown event: " + eventName);
                    return null;
                }
            }
        }

        /// <summary>
        /// Listens to schema for a specific ElementJs instance.
        /// </summary>
        /// <param name="element"></param>
        private void WatchElement(Element element)
        {
            ElementJs elementjs = new ElementJs(null, null, _engine, element);

            Action<ElementSchemaProp<bool>, bool, bool> triggerChange = (prop, old, @new) =>
            {
                UpdateElement(elementjs);
            };
            Action<ElementSchemaProp<float>, float, float> radiusChange = (prop, old, @new) =>
            {
                UpdateElement(elementjs);
            };

            // Subscribe to schema changes.
            element.Schema.Get<bool>(PROP_PROXIMITY_TRIGGER).OnChanged += triggerChange;
            element.Schema.Get<float>(PROP_PROXIMITY_INNER).OnChanged += radiusChange;
            element.Schema.Get<float>(PROP_PROXIMITY_OUTER).OnChanged += radiusChange;

            // Cleanup schema listeners on element's destruction.
            element.OnDestroyed += elm =>
            {
                element.Schema.Get<bool>(PROP_PROXIMITY_TRIGGER).OnChanged -= triggerChange;
                element.Schema.Get<float>(PROP_PROXIMITY_INNER).OnChanged -= radiusChange;
                element.Schema.Get<float>(PROP_PROXIMITY_OUTER).OnChanged -= radiusChange;
            };

            // Trigger initial update.
            UpdateElement(elementjs);
        }

        /// <summary>
        /// Updates ProximityChecker with the current schema for an ElementJs instance.
        /// </summary>
        /// <param name="element"></param>
        private void UpdateElement(ElementJs element)
        {
            var isListening = ElementIsListening(element);
            var isTrigger = ElementIsTrigger(element);

            _proximityChecker.SetElementState(element, isListening, isTrigger);
            if (isListening || isTrigger)
            {
                _proximityChecker.SetElementRadii(
                    element, 
                    element.schema.getOwnNumber(PROP_PROXIMITY_INNER),
                    element.schema.getOwnNumber(PROP_PROXIMITY_OUTER));
            }
        }

        /// <summary>
        /// Ensures an incoming JsValue represents an ElementJs. Throws if the argument is invalid.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static ElementJs ConvertJsValue(JsValue value)
        {
            var element = value.As<ObjectWrapper>().Target as ElementJs;
            if (element == null)
            {
                throw new ArgumentException("ProximityManager must be passed an ElementJs instance");
            }
            return element;
        }

        /// <summary>
        /// Tests whether callbacks are waiting for a specific ElementJs.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool ElementIsListening(ElementJs element)
        {
            return _enterCallbacks.ContainsKey(element)
                || _stayCallbacks.ContainsKey(element)
                || _exitCallbacks.ContainsKey(element);
        }

        /// <summary>
        /// Checks whether an element is configured to be a trigger or not.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool ElementIsTrigger(ElementJs element)
        {
            return element.schema.getOwnBool(PROP_PROXIMITY_TRIGGER);
        }

        /// <summary>
        /// Invoke a given callback for a specific event, for a given Listener/Trigger
        /// </summary>
        /// <param name="event">Name of the event.</param>
        /// <param name="listener">The listening entity.</param>
        /// <param name="trigger">The entity that triggered the event.</param>
        private void InvokeCallbacks(string @event, IEntityJs listener, IEntityJs trigger)
        {
            var map = GetMap(@event);

            List<Func<JsValue, JsValue[], JsValue>> callbacks;
            if (map.TryGetValue(listener, out callbacks))
            {
                var @this = JsValue.FromObject(_engine, listener);
                var parameters = new [] { JsValue.FromObject(_engine, trigger) };

                // copy for now, in case an unsubscribe is in the callback
                var copy = callbacks.ToArray();
                for (int i = 0, len = copy.Length; i < len; i++)
                {
                    copy[i](@this, parameters);
                }
                
            }
        }
    }
}
