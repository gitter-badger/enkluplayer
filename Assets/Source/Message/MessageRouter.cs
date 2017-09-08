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
            public int MessageType { get; private set; }

            public bool IsDispatching = false;

            private readonly List<Action<object>> _subscribers = new List<Action<object>>();
            private readonly List<Action<object>> _toUnsubscribe = new List<Action<object>>();

            public SubscriberGroup(int messageType)
            {
                MessageType = messageType;
            }

            public void AddSubscriber(Action<object, Action> subscriber, bool once = false)
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
            }

            public void Publish(object message)
            {
                IsDispatching = true;

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

                IsDispatching = false;

                if (null != aggregate)
                {
                    throw aggregate;
                }
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

        public void SubscribeOnce(
            int messageType,
            Action<Object, Action> subscriber)
        {
            Group(messageType).AddSubscriber(subscriber, true);
        }

        public void SubscribeAll(Action<Object, Action> subscriber)
        {
            _all.AddSubscriber(subscriber);
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

        private SubscriberGroup Group(int messageType)
        {
            SubscriberGroup subscribers = null;
            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                var group = _groups[i];
                if (@group.MessageType == messageType)
                {
                    subscribers = @group;

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