using System;
using CreateAR.Commons.Unity.Http;
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
        /// Bootstraps on the main thread.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Http service.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Worker object.
        /// </summary>
        private WorldScanPipelineWorker _worker;

        /// <summary>
        /// Configuration object.
        /// </summary>
        public WorldScanPipelineConfiguration Configuration { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public WorldScanPipeline(
            IBootstrapper bootstrapper,
            IHttpService http,
            WorldScanPipelineConfiguration config)
        {
            _bootstrapper = bootstrapper;
            _http = http;

            Configuration = config;
        }

        /// <summary>
        /// Starts processing. Calls to <c>Save</c> must follow a Start and 
        /// preceede a Stop. Start/Stop may be called many times.
        /// </summary>
        public void Start(string tag = null)
        {
            if (null != _worker)
            {
                return;
            }

            _worker = new WorldScanPipelineWorker(
                _bootstrapper,
                _http,
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
            new System.Threading.Thread(_worker.Start).Start();
#endif
        }

        /// <summary>
        /// Stops processing.
        /// </summary>
        public void Stop()
        {
            if (null == _worker)
            {
                return;
            }

            _worker.Kill();
            _worker = null;

            // Thread::Join() unnecessary
        }

        /// <summary>
        /// Saves snapshot of objects passed in.
        /// </summary>
        /// <param name="gameObjects">The gameobjects for which to take a snapshot.</param>
        /// <returns></returns>
        public bool Scan(params GameObject[] gameObjects)
        {
            if (null == _worker)
            {
                Log.Warning(this, "Cannot queue scan until Start() is called.");
                return false;
            }

            return _worker.Queue(gameObjects);
        }
    }
}