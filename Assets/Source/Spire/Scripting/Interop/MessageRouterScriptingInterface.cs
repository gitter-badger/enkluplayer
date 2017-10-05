using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using Jint;
using Jint.Native;

namespace CreateAR.SpirePlayer
{
    [JsInterface("events")]
    public class MessageRouterScriptingInterface
    {
        private class SubscribeRecord
        {
            public readonly string Id;
            public readonly Action Unsubscribe;

            public SubscribeRecord(Action unsub)
            {
                Id = Guid.NewGuid().ToString();
                Unsubscribe = unsub;
            }
        }

        private readonly IMessageRouter _messages;
        private readonly List<SubscribeRecord> _records = new List<SubscribeRecord>();

        public MessageRouterScriptingInterface(IMessageRouter messages)
        {
            _messages = messages;
        }

        public void Publish(int type, string value)
        {
            _messages.Publish(type, value);
        }

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