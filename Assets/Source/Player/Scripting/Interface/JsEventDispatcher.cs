using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Basic implementation of <c>IJsEventDispatcher</c>.
    /// </summary>
    public class JsEventDispatcher : IJsEventDispatcher
    {
        /// <summary>
        /// For bookkeeping.
        /// </summary>
        public class EventListenerRecord
        {
            /// <summary>
            /// Engine that is listening.
            /// </summary>
            public Engine Engine;

            /// <summary>
            /// The handler.
            /// </summary>
            public Func<JsValue, JsValue[], JsValue> Handler;
        }

        /// <summary>
        /// Used to track events.
        /// </summary>
        private readonly Dictionary<string, List<EventListenerRecord>> _events = new Dictionary<string, List<EventListenerRecord>>();
        
        /// <inheritdoc />
        public void on(Engine engine, string eventType, Func<JsValue, JsValue[], JsValue> fn)
        {
            EventList(eventType).Add(new EventListenerRecord
            {
                Engine = engine,
                Handler = fn
            });
        }

        /// <inheritdoc />
        public void off(string eventType)
        {
            EventList(eventType).Clear();
        }

        /// <inheritdoc />
        public void off(Engine engine, string eventType, Func<JsValue, JsValue[], JsValue> fn)
        {
            var list = EventList(eventType);
            for (int i = 0, len = list.Count; i < len; i++)
            {
                var record = list[i];
                if (record.Handler == fn)
                {
                    list.RemoveAt(i);

                    break;
                }
            }
        }

        /// <inheritdoc />
        public void dispatch(string eventType, object evt = null)
        {
            var list = EventList(eventType);
            var count = list.Count;
            if (0 == count)
            {
                return;
            }

            var copy = list.ToArray();
            for (var i = 0; i < count; i++)
            {
                var record = copy[i];
                
                record.Handler(
                    JsValue.FromObject(record.Engine, this),
                    null == evt
                        ? new JsValue[0]
                        : new[] { JsValue.FromObject(record.Engine, evt) });
            }
        }

        /// <summary>
        /// Retrieves the list of event handlers for an event type.
        /// </summary>
        /// <param name="eventType">The type.</param>
        /// <returns></returns>
        private List<EventListenerRecord> EventList(string eventType)
        {
            List<EventListenerRecord> list;
            if (!_events.TryGetValue(eventType, out list))
            {
                list = _events[eventType] = new List<EventListenerRecord>();
            }

            return list;
        }
    }
}