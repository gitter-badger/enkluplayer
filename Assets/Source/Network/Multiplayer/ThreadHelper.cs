using System;
using System.Threading;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Functions for threading.
    /// </summary>
    public static class ThreadHelper
    {
#if !NETFX_CORE
        public static Thread SyncStart(Action threadStart)
        {
            var startedEvent = new ManualResetEvent(false);

            var newThread = new Thread(() =>
            {
                startedEvent.Set();

                threadStart();
            })
            {
                IsBackground = true
            };

            newThread.Start();

            // wait
            if (!startedEvent.WaitOne(TimeSpan.FromSeconds(5.0)))
            {
                throw new Exception("Failed to start thread within 5 seconds of updating the thread state to Running.");
            }

            return newThread;
        }
#endif

#if NETFX_CORE
        /// <summary>
        /// Starts a thread and waits for confirmation of start before proceeding.
        /// </summary>
        /// <param name="threadStart">The action to start./param>
        public static void SyncStart(Action threadStart)
        {
            var startedEvent = new ManualResetEvent(false);

            // start thread
            var task = new System.Threading.Tasks.Task(() =>
            {
                startedEvent.Set();

                threadStart();
            }, System.Threading.Tasks.TaskCreationOptions.LongRunning);
            task.Start();

            // wait
            if (!startedEvent.WaitOne(TimeSpan.FromSeconds(5.0)))
            {
                throw new Exception("Failed to start thread within 5 seconds of updating the thread state to Running.");
            }
        }
#endif
    }
}