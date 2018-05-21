using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Binds together a string and int.
    /// </summary>
    public class MessageTypeBinder
    {
        /// <summary>
        /// Binding between message type used by <c>IMessageRouter</c> and the 
        /// type to deserialize the message as.
        /// </summary>
        private class MessageBinding
        {
            /// <summary>
            /// Message type.
            /// </summary>
            public readonly int MessageType;

            /// <summary>
            /// Payload type.
            /// </summary>
            public readonly Type PayloadType;

            /// <summary>
            /// Creates a new binding.
            /// </summary>
            /// <param name="messageType">The message type used by <c>IMessageRouter</c>.</param>
            /// <param name="payloadType">The type of the payload.</param>
            public MessageBinding(int messageType, Type payloadType)
            {
                MessageType = messageType;
                PayloadType = payloadType;
            }
        }

        /// <summary>
        /// Map from event string to binding.
        /// </summary>
        private readonly List<MessageBinding> _messageMap = new List<MessageBinding>();

        /// <summary>
        /// Clears the binder.
        /// </summary>
        public void Clear()
        {
            _messageMap.Clear();
        }

        /// <summary>
        /// Retrieves binding by message type.
        /// </summary>
        /// <param name="messageType">Message type to retrieve binding for.</param>
        /// <returns></returns>
        public Type ByMessageType(int messageType)
        {
            for (int i = 0, len = _messageMap.Count; i < len; i++)
            {
                var map = _messageMap[i];
                if (map.MessageType == messageType)
                {
                    return map.PayloadType;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Binds a message type to a type.
        /// </summary>
        /// <typeparam name="T">The type of the payload.</typeparam>
        /// <param name="messageType">The message type passed through the router.</param>
        public void Add<T>(int messageType)
        {
            Add(messageType, typeof(T));
        }

        /// <summary>
        /// Binds a message type to a type.
        /// </summary>
        /// <param name="messageType">The message type passed through the router.</param>
        /// <param name="payloadType">The type of the payload.</param>
        public void Add(int messageType, Type payloadType)
        {
            if (null != ByMessageType(messageType))
            {
                throw new Exception(string.Format(
                    "MessageType {0} already bound.",
                    messageType));
            }

            _messageMap.Add(new MessageBinding(messageType, payloadType));
        }

        /// <summary>
        /// Unbinds an event. See Bind.
        /// </summary>
        public void Remove(int messageType)
        {
            Log.Info(this, "Remove({0})", messageType);

            if (null == ByMessageType(messageType))
            {
                throw new Exception(string.Format(
                    "MessageType {0} not bound.",
                    messageType));
            }

            for (int i = 0, len = _messageMap.Count; i < len; i++)
            {
                if (_messageMap[i].MessageType == messageType)
                {
                    _messageMap.RemoveAt(i);
                    return;
                }
            }
        }
    }
}