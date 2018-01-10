using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Long-running thread that process a scan.
    /// </summary>
    public class WorldScanPipelineWriterThread
    {
        /// <summary>
        /// Internal record-keeping.
        /// </summary>
        internal class WorldScanRecord
        {
            /// <summary>
            /// The ObjExporter snapshot.
            /// </summary>
            public ObjExporterState State;
        }

        /// <summary>
        /// Ms to wait for lock.
        /// </summary>
        private readonly int _lockTimeoutMs;

        /// <summary>
        /// Maximum number of scans to allow in the queue.
        /// </summary>
        private readonly int _maxQueued;

        /// <summary>
        /// World scan queue.
        /// </summary>
        private readonly Queue<WorldScanRecord> _queue = new Queue<WorldScanRecord>();

        /// <summary>
        /// For synchronizing.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Exporter.
        /// </summary>
        private readonly ObjExporter _exporter = new ObjExporter();

        /// <summary>
        /// True iff the thread should be running.
        /// </summary>
        private bool _isAlive = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WorldScanPipelineWriterThread(int lockTimeoutMs, int maxQueued)
        {
            _lockTimeoutMs = lockTimeoutMs;
            _maxQueued = maxQueued;
        }

        /// <summary>
        /// Starts the thread.
        /// </summary>
        public void Start()
        {
            Log.Info(this, "Pipeline thread spipnning up.");
            _isAlive = true;
            
            while (true)
            {
                WorldScanRecord record = null;

                lock (_lock)
                {
                    Monitor.Wait(_lock);

                    if (!_isAlive)
                    {
                        break;
                    }

                    record = _queue.Dequeue();
                }

                // process
                var obj = _exporter.Export(record.State);

                // write to disk

                // send to Trellis

                Log.Info(this, "Exported obj.");
            }

            Log.Info(this, "Pipeline thread was spun down.");
        }
        
        /// <summary>
        /// Queues a scan.
        /// </summary>
        /// <param name="objects">Objects to scan.</param>
        /// <returns></returns>
        public bool Queue(GameObject[] objects)
        {
            if (Monitor.TryEnter(_lock, _lockTimeoutMs))
            {
                try
                {
                    _queue.Enqueue(new WorldScanRecord
                    {
                        State = new ObjExporterState(objects)
                    });

                    // discard
                    while (_queue.Count > _maxQueued)
                    {
                        _queue.Dequeue();
                    }

                    Monitor.Pulse(_lock);
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }

            return false;
        }

        /// <summary>
        /// Kills the thread.
        /// </summary>
        public void Kill()
        {
            _isAlive = false;

            // may be waiting on a pulse
            Monitor.Enter(_lock);
            try
            {
                Monitor.Pulse(_lock);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }
    }
}