using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Binds together a string, int, and type.
    /// </summary>
    public class DataBinder
    {
        /// <summary>
        /// Provides a binding for events.
        /// </summary>
        public class Binding
        {
            public string MessageTypeString;
            public int MessageTypeInt;
            public Type Type;
        }

        /// <summary>
        /// Map from event string to binding.
        /// </summary>
        private readonly Dictionary<string, Binding> _messageMap = new Dictionary<string, Binding>();

        /// <summary>
        /// Retrieves binding by message type.
        /// </summary>
        /// <param name="messageTypeString">Message type to retrieve binding for.</param>
        /// <returns></returns>
        public Binding ByMessageType(string messageTypeString)
        {
            Binding binding;
            _messageMap.TryGetValue(messageTypeString, out binding);
            return binding;
        }

        /// <summary>
        /// Binds a message type to a Type.
        /// </summary>
        /// <typeparam name="T">The type with which to parse the event.</typeparam>
        /// <param name="messageTypeString">The message type.</param>
        /// <param name="messageTypeInt">The message type to push onto the <c>IMessageRouter</c>.</param>
        public void Add<T>(string messageTypeString, int messageTypeInt)
        {
            if (_messageMap.ContainsKey(messageTypeString))
            {
                throw new Exception(string.Format(
                    "MessageType {0} already bound.",
                    messageTypeString));
            }

            _messageMap[messageTypeString] = new Binding
            {
                MessageTypeString = messageTypeString,
                MessageTypeInt = messageTypeInt,
                Type = typeof(T)
            };
        }

        /// <summary>
        /// Unbinds an event. See Bind.
        /// </summary>
        public void Remove<T>(string messageTypeString, int messageTypeInt)
        {
            if (!_messageMap.ContainsKey(messageTypeString))
            {
                throw new Exception(string.Format(
                    "MessageType {0} not bound.",
                    messageTypeString));
            }

            _messageMap.Remove(messageTypeString);
        }
    }
}