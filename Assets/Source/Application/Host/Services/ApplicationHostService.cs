using System;
using System.Collections.Generic;
using System.Diagnostics;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a long-running service that listens to the <c>IApplicationHost</c>
    /// implementation.
    /// </summary>
    public abstract class ApplicationHostService
    {
        /// <summary>
        /// <c>IBridge</c> implementation.
        /// </summary>
        protected readonly IBridge _bridge;

        /// <summary>
        /// <c>IMessageRouter</c> implementation.
        /// </summary>
        protected readonly IMessageRouter _messages;

        /// <summary>
        /// List of methods to unsubscribe.
        /// </summary>
        private readonly List<Action> _unsubscribeList = new List<Action>();

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ApplicationHostService(
            IBridge bridge,
            IMessageRouter messages)
        {
            _bridge = bridge;
            _messages = messages;
        }

        /// <summary>
        /// Called when the <c>IApplicationHost</c> starts the application.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Called when the <c>IApplicationHost</c> stops the application.
        /// </summary>
        public void Stop()
        {
            for (int i = 0, len = _unsubscribeList.Count; i < len; i++)
            {
                _unsubscribeList[i]();
            }
            _unsubscribeList.Clear();
        }

        /// <summary>
        /// Adds a binding + a message handler for a messagetype.
        /// </summary>
        /// <typeparam name="T">Type to deserialize the message into.</typeparam>
        /// <param name="messageType">Type of message.</param>
        /// <param name="handler">Handler to handle the message.</param>
        protected void Subscribe<T>(int messageType, Action<T> handler)
        {
            _bridge.Binder.Add<T>(messageType);

            _unsubscribeList.Add(_messages.Subscribe(
                messageType,
                @event => handler((T)@event)));
        }

        /// <summary>
        /// Silly level logging.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        protected void Silly(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}