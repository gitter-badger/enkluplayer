﻿using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
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
        /// Available event names that will be available thru the JsApi
        /// </summary>
        public class ProximityEvents
        {
            /// <summary>
            /// Fired when a trigger enters a subscribed element's inner radius.
            /// </summary>
            public const string enter = "enter";
            
            /// <summary>
            /// Fired every frame after a trigger enters an element, but hasn't exited yet.
            /// </summary>
            public const string stay = "stay";

            /// <summary>
            /// Fired when a trigger exits a subscribed element's outer radius.
            /// </summary>
            public const string exit = "exit";
        }

        /// <summary>
        /// Proximity events that the JsApi can script against.
        /// </summary>
        public ProximityEvents events = new ProximityEvents();

        public const string PROXIMITY_INNER = "proximity.innerRadius";
        public const string PROXIMITY_OUTER = "proximity.outerRadius";
        public const string PROXIMITY_TRIGGER = "proximity.trigger";

        [Inject]
        private IElementManager _elementManager { get; set; }

        private ProximityChecker _proximityChecker = new ProximityChecker();

        /// <summary>
        /// Mapping of subscribed ElementJs->callbacks to be invoked when objects enter proximity.
        /// </summary>
        private readonly Dictionary<ElementJs, Action<ElementJs>> enterCallbacks = new Dictionary<ElementJs, Action<ElementJs>>();

        /// <summary>
        /// Mapping of subscribed ElementJs->callbacks to be invoked every Update while objects are within proximity.
        /// </summary>
        private readonly Dictionary<ElementJs, Action<ElementJs>> stayCallbacks = new Dictionary<ElementJs, Action<ElementJs>>();

        /// <summary>
        /// Mapping of subscribed ElementJs->callbacks to be invoked when objects exit proximity.
        /// </summary>
        private readonly Dictionary<ElementJs, Action<ElementJs>> exitCallbacks = new Dictionary<ElementJs, Action<ElementJs>>();


        protected void Awake()
        {
            base.Awake();
            // TODO: Listen to IElementManager creations, wrap to ElementJs, and watch
        }

        /// <summary>
        /// Called from JsApi. Subscribes an Element to listen for a specified event caused from any trigger.
        /// </summary>
        /// <param name="jsValue">Element that will listen for events</param>
        /// <param name="eventName"><see cref="ProximityEvents"/> event that will be listened for.</param>
        /// <param name="callback">JS callback to invoke on proximity changes</param>
        public void subscribe(JsValue jsValue, string eventName, Action<ElementJs> callback)
        {
            ElementJs element = ConvertJsValue(jsValue);

            switch(eventName) {
                case ProximityEvents.enter:
                    enterCallbacks[element] = callback;
                    break;
                case ProximityEvents.stay:
                    stayCallbacks[element] = callback;
                    break;
                case ProximityEvents.exit:
                    exitCallbacks[element] = callback;
                    break;
                default:
                    Log.Error(this, "Attempted to subscribe to unknown event: " + eventName);
                    return;
            }

            _proximityChecker.SetElementState(element, true, ElementIsTrigger(element));
        }

        /// <summary>
        /// Called from JsApi. Unsubscribes an Element from listening for a specified event caused from any trigger.
        /// </summary>
        /// <param name="jsValue">Element that will be unsubscribed</param>
        /// <param name="eventName"><see cref="ProximityEvents"/> event to unsubscribe from.</param>
        /// <param name="callback">JS callback to unsubscribe</param>
        public void unsubscribe(JsValue jsValue, string eventName, Action callback)
        {
            ElementJs element = ConvertJsValue(jsValue);

            switch(eventName) {
                case ProximityEvents.enter:
                    enterCallbacks.Remove(element);
                    break;
                case ProximityEvents.stay:
                    stayCallbacks.Remove(element);
                    break;
                case ProximityEvents.exit:
                    exitCallbacks.Remove(element);
                    break;
                default:
                    throw new ArgumentException("Attempted to unsubscribe against an unknown event");
            }

            // Check if element is listening - it could have been subscribed to multiple events
            _proximityChecker.SetElementState(element, ElementIsListening(element), ElementIsTrigger(element));
        }

        /// <summary>
        /// Called from Unity. Nudges ProximityChecker to do its job.
        /// </summary>
        private void Update()
        {
            _proximityChecker.Update();
        }

        /// <summary>
        /// Listens to schema for a specific ElementJs instance.
        /// </summary>
        /// <param name="element"></param>
        private void WatchElement(ElementJs element)
        {
            Func<JsValue, JsValue[], JsValue> callback = (jsValue, args) => {
                UpdateElement(element);
                return null;
            };

            element.schema.watchBool(PROXIMITY_TRIGGER, callback);
            element.schema.watchNumber(PROXIMITY_INNER, callback);
            element.schema.watchNumber(PROXIMITY_OUTER, callback);
        }

        /// <summary>
        /// Updates ProximityChecker with the current schema for an ElementJs instance.
        /// </summary>
        /// <param name="element"></param>
        private void UpdateElement(ElementJs element)
        {
            bool isListening = ElementIsListening(element);
            bool isTrigger = ElementIsTrigger(element);

            _proximityChecker.SetElementState(element, isListening, isTrigger);
            if (isListening || isTrigger) _proximityChecker.SetElementRadii(element, element.schema.getOwnNumber(PROXIMITY_INNER), element.schema.getOwnNumber(PROXIMITY_OUTER));
        }

        /// <summary>
        /// Ensures an incoming JsValue represents an ElementJs. Throws if the argument is invalid.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static ElementJs ConvertJsValue(JsValue value)
        {
            ElementJs element = value.As<ObjectWrapper>().Target as ElementJs;
            if(element == null) {
                throw new ArgumentException("ProximityManager.subscribe must be passed an ElementJs instance");
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
            return enterCallbacks.ContainsKey(element)
                || stayCallbacks.ContainsKey(element)
                || exitCallbacks.ContainsKey(element);
        }

        /// <summary>
        /// Checks whether an element is configured to be a trigger or not.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool ElementIsTrigger(ElementJs element)
        {
            return element.schema.getBool(PROXIMITY_TRIGGER);
        }

        private void InvokeCallbacks(string @event, ElementJs elementA, ElementJs elementB)
        {
            Dictionary<ElementJs, Action<ElementJs>> callbackLookup;
            switch(@event) {
                case ProximityEvents.enter:
                    callbackLookup = enterCallbacks;
                    break;
                case ProximityEvents.stay:
                    callbackLookup = stayCallbacks;
                    break;
                case ProximityEvents.exit:
                    callbackLookup = exitCallbacks;
                    break;
                default:
                    throw new ArgumentException("No callback containers exist for Proximity event " + @event);
            }

            var callbacks = callbackLookup[elementA];
            if(callbacks != null) callbacks(elementB);

            callbacks = callbackLookup[elementB];
            if(callbacks != null) callbacks(elementA);
        }
    }
}
