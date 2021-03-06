﻿using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using Enklu.Orchid;
using CreateAR.Commons.Unity.Messaging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// JS interface for messages.
    ///
    /// TODO: Call callbacks with same this as was called with.
    /// </summary>
    [JsInterface("messages")]
    public class MessagingJsInterface
    {
        /// <summary>
        /// Records a subscription to an event.
        /// </summary>
        private class SubscriptionRecord
        {
            /// <summary>
            /// The type of event.
            /// </summary>
            public readonly string EventType;

            /// <summary>
            /// Callback to call.
            /// </summary>
            public readonly IJsCallback Callback;

            /// <summary>
            /// Action to unsubscribe.
            /// </summary>
            public readonly Action Unsubscribe;

            /// <summary>
            /// Constructor.
            /// </summary>
            public SubscriptionRecord(string eventType, IJsCallback callback, Action unsubscribe)
            {
                EventType = eventType;
                Callback = callback;
                Unsubscribe = unsubscribe;
            }
        }

        /// <summary>
        /// Messages implementation specifically set aside for JS.
        /// </summary>
        private readonly JsMessageRouter _messages;

        /// <summary>
        /// List of subscriptions.
        /// </summary>
        private readonly List<SubscriptionRecord> _records = new List<SubscriptionRecord>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessagingJsInterface(IMessageRouter systemRouter, JsMessageRouter jsRouter)
        {
            _messages = jsRouter;

            systemRouter.Subscribe(
                MessageTypes.APPLICATION_RESUME,
                _ => dispatch("system.resume"));
        }

        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="eventType">The event type to listen for.</param>
        /// <param name="callback">The callback to call.</param>
        public void on(string eventType, IJsCallback callback)
        {
            var unsubscribe = _messages.Subscribe(
                ToMessageType(eventType),
                evt =>
                {
                    try
                    {
                        callback.Apply(this, evt);
                    }
                    catch (Exception exception)
                    {
                        Log.Warning(this, "JavaScript error : {0}.", exception);
                    }
                });

            var record = new SubscriptionRecord(eventType, callback, unsubscribe);

            _records.Add(record);
        }

        /// <summary>
        /// Removes an event listener.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="callback">The callback.</param>
        public void off(string eventType, IJsCallback callback)
        {
            for (var i = _records.Count - 1; i >= 0; i--)
            {
                var record = _records[i];
                if (record.EventType == eventType && record.Callback == callback)
                {
                    _records.RemoveAt(i);

                    record.Unsubscribe();
                }
            }
        }

        /// <summary>
        /// Dispatches an event.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        public void dispatch(string eventType)
        {
            dispatch(eventType, Void.Instance);
        }

        /// <summary>
        /// Dispatches an event.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="value">The event.</param>
        public void dispatch(string eventType, object value)
        {
            try
            {
                _messages.Publish(ToMessageType(eventType), value);
            }
            catch (Exception ex)
            {
                Log.Warning(this, "Error dispatching event : {0}.", ex);
            }
        }

        /// <summary>
        /// Converts a string to an int with decent hash collision characteristics.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <returns></returns>
        private static int ToMessageType(string eventType)
        {
            // this is a hash, so theoretically we could see collisions
            return eventType.GetHashCode();
        }
    }
}