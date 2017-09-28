using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Binds together a string and int.
    /// </summary>
    public class MessageTypeBinder
    {
        /// <summary>
        /// Map from event string to binding.
        /// </summary>
        private readonly List<Commons.Unity.DataStructures.Tuple<string, int>> _messageMap = new List<Commons.Unity.DataStructures.Tuple<string, int>>();

        /// <summary>
        /// Retrieves binding by message type.
        /// </summary>
        /// <param name="messageTypeString">Message type to retrieve binding for.</param>
        /// <returns></returns>
        public int ByMessageType(string messageTypeString)
        {
            for (int i = 0, len = _messageMap.Count; i < len; i++)
            {
                var map = _messageMap[i];
                if (map.Item1 == messageTypeString)
                {
                    return map.Item2;
                }
            }
            
            return -1;
        }

        /// <summary>
        /// Binds a message type to a Type.
        /// </summary>
        /// <param name="messageTypeString">The message type.</param>
        /// <param name="messageTypeInt">The message type to push onto the <c>IMessageRouter</c>.</param>
        public void Add(string messageTypeString, int messageTypeInt)
        {
            if (-1 != ByMessageType(messageTypeString))
            {
                throw new Exception(string.Format(
                    "MessageType {0} already bound.",
                    messageTypeString));
            }

            _messageMap.Add(Commons.Unity.DataStructures.Tuple.Create(messageTypeString, messageTypeInt));
        }

        /// <summary>
        /// Unbinds an event. See Bind.
        /// </summary>
        public void Remove(string messageTypeString)
        {
            if (-1 == ByMessageType(messageTypeString))
            {
                throw new Exception(string.Format(
                    "MessageType {0} not bound.",
                    messageTypeString));
            }

            for (int i = 0, len = _messageMap.Count; i < len; i++)
            {
                if (_messageMap[i].Item1 == messageTypeString)
                {
                    _messageMap.RemoveAt(i);
                    return;
                }
            }
        }
    }
}