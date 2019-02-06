using System;
using System.Threading;

namespace CreateAR.EnkluPlayer
{
    public static class ThreadHelper
    {
        public static Thread SyncStart(Action threadStart, bool isBackground)
        {
            var startedEvent = new ManualResetEvent(false);
            var newThread = new Thread(() =>
                {
                    startedEvent.Set();

                    threadStart();
                })
                { IsBackground = isBackground };

            newThread.Start();

            if (!startedEvent.WaitOne(TimeSpan.FromSeconds(5.0)))
            {
                throw new Exception("Failed to start thread within 5 seconds of updating the threaed state to Running");
            }

            return newThread;
        }
    }
}