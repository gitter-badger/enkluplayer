using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Basic implementation of <c>IJsEventDispatcher</c>.
    /// </summary>
    public class JsEventDispatcher : IJsEventDispatcher
    {
        /// <summary>
        /// Used to track events.
        /// </summary>
        private readonly Dictionary<string, List<IJsCallback>> _events = new Dictionary<string, List<IJsCallback>>();

        /// <inheritdoc />
        public void on(string eventType, IJsCallback fn)
        {
            EventList(eventType).Add(fn);
        }

        /// <inheritdoc />
        public void off(string eventType)
        {
            EventList(eventType).Clear();
        }

        /// <inheritdoc />
        public void off(string eventType, IJsCallback fn)
        {
            var list = EventList(eventType);
            for (int i = 0, len = list.Count; i < len; i++)
            {
                if (list[i] == fn)
                {
                    list.RemoveAt(i);

                    break;
                }
            }
        }

        /// <inheritdoc />
        public void dispatch(string eventType)
        {
            dispatch(eventType, null);
        }

        /// <inheritdoc />
        public void dispatch(string eventType, object evt)
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
                var callback = copy[i];

                try
                {
                    if (null == evt)
                    {
                        callback.Apply(this);
                    }
                    else
                    {
                        callback.Apply(this, evt);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(this, "Exception dispatching event {0} : {1}.", eventType, exception);
                }
            }
        }

        /// <summary>
        /// Retrieves the list of event handlers for an event type.
        /// </summary>
        /// <param name="eventType">The type.</param>
        /// <returns></returns>
        private List<IJsCallback> EventList(string eventType)
        {
            List<IJsCallback> list;
            if (!_events.TryGetValue(eventType, out list))
            {
                list = _events[eventType] = new List<IJsCallback>();
            }

            return list;
        }
    }
}