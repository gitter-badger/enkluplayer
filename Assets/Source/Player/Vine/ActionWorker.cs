using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer.Vine
{
    /// <summary>
    /// Generic worker.
    /// </summary>
    public class ActionWorker
    {
        /// <summary>
        /// Input queue.
        /// </summary>
        private readonly Queue<Action> _queue = new Queue<Action>();

        /// <summary>
        /// Mailbox for main thread.
        /// </summary>
        private readonly Queue<Action> _mailbox = new Queue<Action>();

        /// <summary>
        /// Temporary queue for deliveries.
        /// </summary>
        private readonly List<Action> _deliveries = new List<Action>();

        /// <summary>
        /// Lock for queue.
        /// </summary>
        private readonly object _queueLock = new object();

        /// <summary>
        /// True iff alive.
        /// </summary>
        private bool _isAlive;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ActionWorker(IBootstrapper bootstrapper)
        {
            bootstrapper.BootstrapCoroutine(Poll());
        }

        /// <summary>
        /// Starts worker.
        /// </summary>
        public void Start()
        {
            Log.Info(this, "Starting ActionWorker thread.");

            _isAlive = true;

            lock (_queueLock)
            {
                while (_isAlive)
                {
                    Monitor.Wait(_queueLock);

                    if (!_isAlive)
                    {
                        break;
                    }

                    Log.Info(this, "ActionWorker->Next()");

                    var action = _queue.Dequeue();
                    action();
                }
            }
        }

        /// <summary>
        /// Queues an action.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="callback">The callback.</param>
        public void Enqueue<T>(Func<T> action, Action<T> callback)
        {
            Action act = () =>
            {
                var value = action();

                lock (_mailbox)
                {
                    _mailbox.Enqueue(() => callback(value));
                }
            };

            lock (_queueLock)
            {
                _queue.Enqueue(act);

                Monitor.Pulse(_queueLock);
            }
        }

        /// <summary>
        /// Runs on main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Poll()
        {
            while (true)
            {
                lock (_mailbox)
                {
                    _deliveries.AddRange(_mailbox);
                    _mailbox.Clear();
                }

                for (int i = 0, len = _deliveries.Count; i < len; i++)
                {
                    _deliveries[i]();
                }

                _deliveries.Clear();

                yield return null;
            }
        }
    }
}