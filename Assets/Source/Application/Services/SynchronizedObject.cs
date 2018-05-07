using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    public class SynchronizedObject<T>
    {
        private readonly Queue<Action<T, Action<T>>> _mods = new Queue<Action<T, Action<T>>>();
        private readonly Action<T, Action> _subscriber;

        private bool _isProcessing = false;
        private bool _isWaitingOnSubscriber = false;

        public T Data { get; private set; }
        
        public SynchronizedObject(T data, Action<T, Action> subscriber)
        {
            Data = data;

            _subscriber = subscriber;
        }

        public void Queue(Action<T, Action<T>> mod)
        {
            _mods.Enqueue(mod);

            if (!_isWaitingOnSubscriber)
            {
                ProcessQueue();
            }
        }

        private void ProcessQueue()
        {
            if (_isProcessing)
            {
                return;
            }

            if (_mods.Count == 0)
            {
                return;
            }

            _isProcessing = true;

            var action = _mods.Dequeue();
            action(Data, OnComplete);
        }

        private void OnComplete(T val)
        {
            Data = val;

            _isProcessing = false;
            _isWaitingOnSubscriber = true;
            _subscriber(Data, () =>
            {
                _isWaitingOnSubscriber = false;

                ProcessQueue();
            });
        }
    }
}