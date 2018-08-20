using Jint.Native;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Available event names that will be available thru the JsApi
    /// </summary>
    public class ProximityEvents
    {
        public const string enter = "Proximity.ENTER";
        public const string stay = "Proximity.STAY";
        public const string exit = "Proximity.EXIT";
    }

    /// <summary>
    /// Manages detecting and dispatching proximity events for Elements. Elements can subscribe and unsubscribe 
    ///     for events against specific Elements. Proximity radii (inner/outter) is set through the Element's schema.
    ///     
    /// Elements subscribed for events are considered "listening". Proximity events are sent when "trigger" Elements
    ///     interact with "listening" Elements. ProximityManager will listen to objects for changes to their schema
    ///     to determine if something should be considered a trigger or not.
    /// </summary>
    [JsInterface("proximity")]
    public class ProximityManager : JsEventSystem<ProximityEvents>
    {
        // TODO: Should these be standardized/public similar to ProximityEvents?
        private const string PROXIMITY_INNER = "proximity.innerRadius";
        private const string PROXIMITY_OUTTER = "proximity.outterRadius";
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
