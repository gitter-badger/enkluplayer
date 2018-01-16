using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Util;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Long-running thread that process a scan.
    /// </summary>
    public class WorldScanPipelineWorker
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
        /// Writes a max of N versions of the same file to disk.
        /// </summary>
        private readonly VersionedFileWriter _fileWriter;

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
        private readonly ObjExporter _exporter = new ObjExporter();

        /// <summary>
        /// True iff the thread should be running.
        /// </summary>
        private bool _isAlive = false;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public WorldScanPipelineWorker(
            IBootstrapper bootstrapper,
            IHttpService http,
            int lockTimeoutMs,
            int maxQueued,
            int maxOnDisk,
            string tag)
        {
            _lockTimeoutMs = lockTimeoutMs;
            _maxQueued = maxQueued;
            
            var folder = Path.Combine(
                Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    "Scans"),
                tag);

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            Directory.CreateDirectory(folder);

            _fileWriter = new VersionedFileWriter(
                folder,
                tag,
                "obj",
                maxOnDisk);
            _fileUploader = new FileResourceUpdater(
                bootstrapper,
                http,
                "worldscan");

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
                var bytes = Encoding.UTF8.GetBytes(obj);

                // write to disk
                _fileWriter.Write(bytes);

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