using System;
using System.Collections.Generic;
using System.Threading;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Long-running thread that process a scan.
    /// </summary>
    public class MeshCaptureExportServiceWorker
    {
        /// <summary>
        /// Internal record-keeping.
        /// </summary>
        internal class WorldScanRecord
        {
            /// <summary>
            /// The ObjExporter snapshot.
            /// </summary>
            public MeshStateCollection State;
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
        /// Uploads a file over and over, serially.
        /// </summary>
        private readonly FileResourceUpdater _fileUploader;
        
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
        private readonly MeshExporter _exporter = new MeshExporter();

        /// <summary>
        /// True iff the thread should be running.
        /// </summary>
        private bool _isAlive;

        /// <summary>
        /// Called when file url is updated.
        /// </summary>
        public event Action<string> OnFileUrlChanged;

        /// <summary>
        /// Called when file is created.
        /// </summary>
        public event Action<string> OnFileCreated;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshCaptureExportServiceWorker(
            IBootstrapper bootstrapper,
            IHttpService http,
            int lockTimeoutMs,
            int maxQueued,
            string tag,
            string fileId)
        {
            _lockTimeoutMs = lockTimeoutMs;
            _maxQueued = maxQueued;
            
            _fileUploader = new FileResourceUpdater(
                bootstrapper,
                http,
                tag,
                fileId);
            _fileUploader.OnFileUrlChanged += url =>
            {
                if (null != OnFileUrlChanged)
                {
                    OnFileUrlChanged(url);
                }
            };
            _fileUploader.OnFileCreated += id =>
            {
                if (null != OnFileCreated)
                {
                    OnFileCreated(id);
                }
            };
            _fileUploader.Start();
        }

        /// <summary>
        /// Starts the thread.
        /// </summary>
        public void Start()
        {
            Log.Info(this, "Pipeline thread spinning up.");
            _isAlive = true;
            
            while (true)
            {
                WorldScanRecord record;

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
                var bytes = _exporter.Export(record.State);
                
                // send to Trellis
                _fileUploader.Write(bytes);

                Log.Info(this, "Exported obj.");
            }

            Log.Info(this, "Pipeline thread was spun down.");
        }

        /// <summary>
        /// Queues a scan.
        /// </summary>
        /// <param name="objects">Objects to scan.</param>
        /// <param name="triangles">Number of triangles</param>
        /// <returns></returns>
        public bool Queue(GameObject[] objects, out int triangles)
        {
            MeshStateCollection state = null;

            if (Monitor.TryEnter(_lock, _lockTimeoutMs))
            {
                try
                {
                    state = new MeshStateCollection(objects);
                    _queue.Enqueue(new WorldScanRecord
                    {
                        State = state
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

            if (null == state)
            {
                triangles = 0;
                return false;
            }

            triangles = state.Triangles;
            return true;
        }

        /// <summary>
        /// Kills the thread.
        /// </summary>
        public void Kill()
        {
            _isAlive = false;

            _fileUploader.Stop();

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