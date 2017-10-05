using System;
using CreateAR.Commons.Unity.Messaging;
using Jint;
using Jint.Native;

namespace CreateAR.SpirePlayer
{
    [JsInterface("events")]
    public class MessageRouterScriptingInterface
    {
        private readonly IMessageRouter _messages;

        public MessageRouterScriptingInterface(IMessageRouter messages)
        {
            _messages = messages;
        }

        public void Publish(int type, string value)
        {
            _messages.Publish(type, value);
        }

        public void Subscribe(
            Engine engine,
            int type,
            Func<JsValue, JsValue[], JsValue> callback)
        {
            _messages.Subscribe(
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
        }
    }
}