using System;
using System.Threading;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Functions for threading.
    /// </summary>
    public static class ThreadHelper
    {
        /// <summary>
        /// Starts a thread and waits for confirmation of start before proceeding.
        /// </summary>
        /// <param name="threadStart">The action to start./param>
        public static void SyncStart(Action threadStart)
        {
            var startedEvent = new ManualResetEvent(false);

            // start thread
#if NETFX_CORE
            var task = new System.Threading.Tasks.Task(() =>
            {
                startedEvent.Set();

                threadStart();
            }, System.Threading.Tasks.TaskCreationOptions.LongRunning);
            task.Start();
#else
            var newThread = new Thread(() =>
                {
                    startedEvent.Set();

                    threadStart();
                })
                { IsBackground = true };

            newThread.Start();
#endif

            // wait
            if (!startedEvent.WaitOne(TimeSpan.FromSeconds(5.0)))
            {
                throw new Exception("Failed to start thread within 5 seconds of updating the thread state to Running.");
            }
        }
    }
}