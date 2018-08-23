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

        // TODO: Should these be standardized/public similar to ProximityEvents?
        private const string PROXIMITY_INNER = "proximity.innerRadius";
        private const string PROXIMITY_OUTER = "proximity.outerRadius";
        private const string PROXIMITY_TRIGGER = "proximity.trigger";

        /// <summary>
        /// Called from JsApi. Subscribes an Element to listen for a specified event caused from any trigger.
        /// </summary>
        /// <param name="jsValue">Element that will listen for events</param>
        /// <param name="eventName"><see cref="ProximityEvents"/> event that will be listened for.</param>
        /// <param name="callback">JS callback to invoke on proximity changes</param>
        public void subscribe(JsValue jsValue, string eventName, Action<ElementJs> callback)
        {

        }

        /// <summary>
        /// Called from JsApi. Unsubscribes an Element from listening for a specified event caused from any trigger.
        /// </summary>
        /// <param name="jsValue">Element that will be unsubscribed</param>
        /// <param name="eventName"><see cref="ProximityEvents"/> event to unsubscribe from.</param>
        /// <param name="callback">JS callback to unsubscribe</param>
        public void unsubscribe(JsValue jsValue, string eventName, Action callback)
        {

        }

        /// <summary>
        /// Checks whether any proximity events should be dispatched or not.
        /// </summary>
        private void Update()
        {
            
        }
    }
}
