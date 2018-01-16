using System;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Async pipeline that takes world mesh scans.
    /// </summary>
    public class WorldScanPipeline
    {
        /// <summary>
        /// Writer thread.
        /// </summary>
        private WorldScanPipelineWriterThread _writer;

        /// <summary>
        /// Configuration object.
        /// </summary>
        public WorldScanPipelineConfiguration Configuration { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public WorldScanPipeline(WorldScanPipelineConfiguration config)
        {
            Configuration = config;
        }

        /// <summary>
        /// Starts processing. Calls to <c>Save</c> must follow a Start and 
        /// preceede a Stop. Start/Stop may be called many times.
        /// </summary>
        public void Start(string tag = null)
        {
            if (null != _writer)
            {
                return;
            }

            _writer = new WorldScanPipelineWriterThread(
                Configuration.LockTimeoutMs,
                Configuration.MaxScanQueueLen,
                Configuration.MaxOnDisk,
                tag ?? Guid.NewGuid().ToString());

#if NETFX_CORE
            // here
            System.Threading.Tasks.Task.Factory.StartNew(
                _writer.Start,
                System.Threading.Tasks.TaskCreationOptions.LongRunning);
#else
            new System.Threading.Thread(_writer.Start).Start();
#endif
        }

        /// <summary>
        /// Stops processing.
        /// </summary>
        public void Stop()
        {
            if (null == _writer)
            {
                return;
            }

            _writer.Kill();
            _writer = null;

            // Thread::Join() unnecessary
        }

        /// <summary>
        /// Saves snapshot of objects passed in.
        /// </summary>
        /// <param name="gameObjects">The gameobjects for which to take a snapshot.</param>
        /// <returns></returns>
        public bool Scan(params GameObject[] gameObjects)
        {
            if (null == _writer)
            {
                Log.Warning(this, "Cannot queue scan until Start() is called.");
                return false;
            }

            return _writer.Queue(gameObjects);
        }
    }
}