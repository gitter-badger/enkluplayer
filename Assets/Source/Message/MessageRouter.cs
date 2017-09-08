using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class MessageRouter
    {
        private class SubscriberGroup
        {
            private readonly List<Action<object>> _subscribers = new List<Action<object>>();
            private readonly List<Action<object>> _toUnsubscribe = new List<Action<object>>();
            private object _message;
            private bool _isAborted = false;

            public int MessageType { get; private set; }
            public bool IsDispatching { get; private set; }

            public object Message {
                get
                {
                    return _message;
                }
                set
                {
                    _message = value;

                    IsDispatching = null != _message;

                    if (null == value)
                    {
                        _isAborted = false;
                    }
                }
            }

            public SubscriberGroup(int messageType)
            {
                MessageType = messageType;
            }
            
            public Action AddSubscriber(Action<object, Action> subscriber, bool once = false)
            {
                Action unsub = null;
                Action<object> action = message =>
                {
                    subscriber(message, unsub);

                    if (once)
                    {
                        unsub();
                    }
                };

                unsub = () =>
                {
                    if (IsDispatching)
                    {
                        _toUnsubscribe.Add(action);
                    }
                    else
                    {
                        _subscribers.Remove(action);
                    }
                };

                _subscribers.Add(action);

                return unsub;
            }

            public void Publish(object message)
            {
                Message = message;

                AggregateException aggregate = null;

                for (int i = 0, len = _subscribers.Count; i < len; i++)
                {
                    try
                    {
                        _subscribers[i](message);
                    }
                    catch (Exception exception)
                    {
                        if (null == aggregate)
                        {
                            aggregate = new AggregateException();
                        }

                        aggregate.Exceptions.Add(exception);
                    }

                    if (_isAborted)
                    {
                        break;
                    }
                }

                // unsubscribes
                var length = _toUnsubscribe.Count;
                if (length > 0)
                {
                    for (var i = 0; i < length; i++)
                    {
                        _subscribers.Remove(_toUnsubscribe[i]);
                    }
                    _toUnsubscribe.Clear();
                }

                Message = null;

                if (null != aggregate)
                {
                    throw aggregate;
                }
            }

            public void Abort()
            {
                _isAborted = true;
            }
        }

        private readonly SubscriberGroup _all = new SubscriberGroup(-1);
        private readonly List<SubscriberGroup> _groups = new List<SubscriberGroup>();

        public void Subscribe(
            int messageType,
            Action<object, Action> subscriber)
        {
            Group(messageType).AddSubscriber(subscriber);
        }

        public Action Subscribe(
            int messageType,
            Action<object> subscriber)
        {
            return Group(messageType)
                .AddSubscriber((message, unsub) => subscriber(message));
        }

        public void SubscribeOnce(
            int messageType,
            Action<object, Action> subscriber)
        {
            Group(messageType).AddSubscriber(subscriber, true);
        }

        public Action SubscribeOnce(
            int messageType,
            Action<object> subscriber)
        {
            return Group(messageType)
                .AddSubscriber((message, unsub) => subscriber(message), true);
        }

        public void SubscribeAll(Action<object, Action> subscriber)
        {
            _all.AddSubscriber(subscriber);
        }

        public Action SubscribeAll(Action<object> subscriber)
        {
            return _all.AddSubscriber((message, unsub) => subscriber(message));
        }

        public void Publish(
            int messageType,
            object message)
        {
            _all.Publish(message);

            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                var group = _groups[i];
                if (group.MessageType == messageType)
                {
                    // cannot publish in the middle of a dispatch
                    if (group.IsDispatching)
                    {
                        Log.Warning(this, "Cyclical publish caught in middle of dispatch and discarded.\n\t[{0}] = {1}",
                            messageType,
                            message);
                        return;
                    }

                    group.Publish(message);

                    break;
                }
            }
        }

        public void Consume(object message)
        {
            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                var group = _groups[i];
                if (group.Message == message)
                {
                    group.Abort();
                }
            }
        }

        private SubscriberGroup Group(int messageType)
        {
            SubscriberGroup subscribers = null;
            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                var group = _groups[i];
                if (group.MessageType == messageType)
                {
                    subscribers = group;

                    break;
                }
            }

            if (null == subscribers)
            {
                subscribers = new SubscriberGroup(messageType);
                _groups.Add(subscribers);
            }
            return subscribers;
        }
    }
}