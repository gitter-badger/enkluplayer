using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Handles IUXEvents from children only.
    /// </summary>
    public class IUXEventHandler : InjectableMonoBehaviour
    {
        /// <summary>
        /// For internal book-keeping. Tracks a type + a list of handlers in a
        /// stack-safe way.
        /// </summary>
        private class HandlerRecord
        {
            /// <summary>
            /// If true, _handlers cannot be touched.
            /// </summary>
            private bool _isLocked = false;

            /// <summary>
            /// List of handlers.
            /// </summary>
            private readonly List<IIUXEventHandler> _handlers = new List<IIUXEventHandler>();

            /// <summary>
            /// List of handlers waiting to be removed.
            /// </summary>
            private readonly List<IIUXEventHandler> _toRemove = new List<IIUXEventHandler>();

            /// <summary>
            /// The MessageType this record refers to.
            /// </summary>
            public readonly int Type;
            
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="type">MessageType.</param>
            public HandlerRecord(int type)
            {
                Type = type;
            }

            /// <summary>
            /// Locks while iterating.
            /// </summary>
            public void Lock()
            {
                _isLocked = true;
            }

            /// <summary>
            /// Unlocks-- free to add/remove.
            /// </summary>
            public void Unlock()
            {
                _isLocked = false;

                Reconcile();
            }
            
            /// <summary>
            /// Adds a handler.
            /// </summary>
            /// <param name="handler">Handler to add.</param>
            public void Add(IIUXEventHandler handler)
            {
                _handlers.Add(handler);
            }

            /// <summary>
            /// Removes a handler.
            /// </summary>
            /// <param name="handler">handler to remove.</param>
            public void Remove(IIUXEventHandler handler)
            {
                if (_isLocked)
                {
                    _toRemove.Add(handler);
                }
                else
                {
                    _handlers.Remove(handler);
                }
            }

            /// <summary>
            /// Calls handlers for a specific event. Stops if a handler consumes
            /// the event.
            /// </summary>
            /// <param name="event">The event.</param>
            /// <returns>True iff the event was consumed.</returns>
            public bool Call(IUXEvent @event)
            {
                for (int i = 0, len = _handlers.Count; i < len; i++)
                {
                    if (_handlers[i].OnEvent(@event))
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// Reconsiles queued removes.
            /// </summary>
            private void Reconcile()
            {
                var len = _toRemove.Count;
                if (0 == len)
                {
                    return;
                }
                
                for (var i = 0; i < len; i++)
                {
                    _handlers.Remove(_toRemove[i]);
                }

                _toRemove.Clear();
            }
        }

        /// <summary>
        /// Unsubscribe action.
        /// </summary>
        private Action _unsub;

        /// <summary>
        /// Tracks who is listening to whom.
        /// </summary>
        private readonly List<HandlerRecord> _handlers = new List<HandlerRecord>();

        /// <summary>
        /// Fires messages.
        /// </summary>
        [Inject]
        public IMessageRouter Messages { get; set; }

        /// <summary>
        /// Adds a handler for a message type.
        /// </summary>
        /// <param name="messageType">The type to listen to.</param>
        /// <param name="handler">The handler to add.</param>
        public void AddHandler(int messageType, IIUXEventHandler handler)
        {
            HandlerRecord record;
            for (int i = 0, len = _handlers.Count; i < len; i++)
            {
                record = _handlers[i];
                if (record.Type == messageType)
                {
                    record.Add(handler);

                    return;
                }
            }

            record = new HandlerRecord(messageType);
            record.Add(handler);

            _handlers.Add(record);
        }

        /// <summary>
        /// Removes a handler from a message type.
        /// </summary>
        /// <param name="messageType">The message type.</param>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveHandler(int messageType, IIUXEventHandler handler)
        {
            for (int i = 0, len = _handlers.Count; i < len; i++)
            {
                var record = _handlers[i];
                if (record.Type == messageType)
                {
                    record.Remove(handler);
                    break;
                }
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnEnable()
        {
            _unsub = Messages.SubscribeAll(Messages_OnSubscribeAll);
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnDisable()
        {
            _unsub();
        }

        /// <summary>
        /// Called every message we receive.
        /// </summary>
        /// <param name="message">The message received.</param>
        private void Messages_OnSubscribeAll(object message)
        {
            var @event = message as IUXEvent;
            if (null == @event)
            {
                return;
            }

            var widget = @event.Target as Widget;
            if (null == widget)
            {
                return;
            }

            var trans = widget.GameObject.transform;
            while (true)
            {
                if (trans == transform)
                {
                    if (Emit(@event))
                    {
                        Messages.Consume(@event);

                        break;
                    }
                }

                trans = trans.parent;

                if (null == trans)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Passes a matching event to handlers.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <returns></returns>
        private bool Emit(IUXEvent @event)
        {
            var consumed = false;
            var type = @event.Type;
            for (int i = 0, len = _handlers.Count; i < len; i++)
            {
                var record = _handlers[i];
                if (record.Type == type)
                {
                    record.Lock();
                    consumed = record.Call(@event);
                    record.Unlock();

                    break;
                }
            }

            return consumed;
        }
    }
}