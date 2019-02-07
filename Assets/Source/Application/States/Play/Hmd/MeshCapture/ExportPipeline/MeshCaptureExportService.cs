using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Async pipeline that takes world mesh scans.
    /// </summary>
    public class MeshCaptureExportService : IMeshCaptureExportService
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
        private MeshCaptureExportServiceWorker _worker;

        /// <summary>
        /// Configuration object.
        /// </summary>
        public MeshCaptureExportServiceConfiguration Configuration { get; private set; }

        /// <inheritdoc />
        public event Action<string> OnFileUrlChanged;

        /// <inheritdoc />
        public event Action<string> OnFileCreated;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshCaptureExportService(
            IBootstrapper bootstrapper,
            IHttpService http,
            MeshCaptureExportServiceConfiguration config)
        {
            _bootstrapper = bootstrapper;
            _http = http;

            Configuration = config;
        }

        /// <inheritdoc />
        public void Start(string appId, string fileId = null)
        {
            if (null != _worker)
            {
                return;
            }

            _worker = new MeshCaptureExportServiceWorker(
                _bootstrapper,
                _http,
                Configuration.LockTimeoutMs,
                Configuration.MaxScanQueueLen,
                string.Format("worldscan,appId:{0}", appId),
                fileId);
            _worker.OnFileUrlChanged += url =>
            {
                if (null != OnFileUrlChanged)
                {
                    OnFileUrlChanged(url);
                }
            };
            _worker.OnFileCreated += id =>
            {
                if (null != OnFileCreated)
                {
                    OnFileCreated(id);
                }
            };

#if NETFX_CORE || (!UNITY_EDITOR && UNITY_WSA)
            // here
            System.Threading.Tasks.Task.Factory.StartNew(
                _worker.Start,
                System.Threading.Tasks.TaskCreationOptions.LongRunning);
#else
            new System.Threading.Thread(_worker.Start).Start();
#endif
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool Export(out int triangles, params GameObject[] gameObjects)
        {
            if (null == _worker)
            {
                Log.Warning(this, "Cannot queue scan until Start() is called.");
                triangles = 0;
                return false;
            }

            return _worker.Queue(gameObjects, out triangles);
        }
    }
}