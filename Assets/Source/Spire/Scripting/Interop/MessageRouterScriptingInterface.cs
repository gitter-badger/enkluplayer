using CreateAR.Commons.Unity.Messaging;
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

        public void Subscribe(int type, ICallable callback)
        {
            _messages.Subscribe(
                type,
                message =>
                {
                    // ?
                });
        }
    }
}