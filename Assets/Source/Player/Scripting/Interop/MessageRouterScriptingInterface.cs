using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// JavaScript interface for IMessageRouter.
    /// </summary>
    [JsInterface("events")]
    public class MessageRouterScriptingInterface
    {
        /// <summary>
        /// Keep track of subscribes so we can unsubscribe.
        /// </summary>
        private class SubscribeRecord
        {
            /// <summary>
            /// Unique id.
            /// </summary>
            public readonly string Id;

            /// <summary>
            /// Unsubscribe action.
            /// </summary>
            public readonly Action Unsubscribe;

            /// <summary>
            /// Consructor.
            /// </summary>
            /// <param name="unsub">Unsubscribe action.</param>
            public SubscribeRecord(Action unsub)
            {
                Id = Guid.NewGuid().ToString();
                Unsubscribe = unsub;
            }
        }

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// List of subscribe records.
        /// </summary>
        private readonly List<SubscribeRecord> _records = new List<SubscribeRecord>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messages">Messages.</param>
        public MessageRouterScriptingInterface(IMessageRouter messages)
        {
            _messages = messages;
        }

        /// <summary>
        /// Publishes a method.
        /// </summary>
        /// <param name="type">Type of message.</param>
        /// <param name="value">Object to publish.</param>
        public void Publish(int type, object value)
        {
            _messages.Publish(type, value);
        }

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="engine">The engine calling this.</param>
        /// <param name="type">The type of event to subscribe to.</param>
        /// <param name="callback">Callback.</param>
        /// <returns></returns>
        public string Subscribe(
            Engine engine,
            int type,
            Func<JsValue, JsValue[], JsValue> callback)
        {
            var unsub = _messages.Subscribe(
                type,
                message =>
                {
                    callback(
                        JsValue.FromObject(engine, engine),
                        new []
                        {
                            JsValue.FromObject(engine, message)
                        });
                });

            var record = new SubscribeRecord(unsub);
            _records.Add(record);
            return record.Id;
        }

        /// <summary>
        /// Unsubscribes from future updates, given an id returned from Subscribe.
        /// </summary>
        /// <param name="id">Unique id returned from Subscribe.</param>
        public void Unsubscribe(string id)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (record.Id == id)
                {
                    _records.RemoveAt(i);

                    record.Unsubscribe();
                    break;
                }
            }
        }
    }
}