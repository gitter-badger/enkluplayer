using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Object that can filter messages.
    /// </summary>
    public class MessageFilter
    {
        /// <summary>
        /// Routes messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Collection of filters.
        /// </summary>
        private readonly List<IMessageExclusionFilter> _exclusionFilters = new List<IMessageExclusionFilter>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageFilter(IMessageRouter messages)
        {
            _messages = messages;
        }

        /// <summary>
        /// Adds a filters.
        /// </summary>
        /// <param name="filter">An exclusion filter.</param>
        /// <returns></returns>
        public MessageFilter Filter(IMessageExclusionFilter filter)
        {
            _exclusionFilters.Add(filter);

            return this;
        }

        /// <summary>
        /// Removes a filter.
        /// </summary>
        /// <param name="filter">An exclusion filter.</param>
        /// <returns></returns>
        public MessageFilter Unfilter(IMessageExclusionFilter filter)
        {
            _exclusionFilters.Remove(filter);

            return this;
        }

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name="messageType">The type of message.</param>
        /// <param name="message">Object to publish.</param>
        public void Publish(int messageType, object message)
        {
            for (int i = 0, len = _exclusionFilters.Count; i < len; i++)
            {
                if (_exclusionFilters[i].Exclude(messageType, message))
                {
                    return;
                }
            }

            _messages.Publish(messageType, message);
        }
    }
}