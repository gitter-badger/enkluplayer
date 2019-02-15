using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Forwards performance metrics to graphite.
    /// </summary>
    public class PerfMetricsCollector
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private IMetricsService _metrics;
        private IDeviceMetaProvider _metaProvider;
        private IPrimaryAnchorManager _primaryAnchorManager;
        private IBootstrapper _bootstrapper;
        private RuntimeStats _runtimeStats;

        /// <summary>
        /// Monitors metrics.
        /// </summary>
        private PerfMonitor _monitor;

        /// <summary>
        /// Cached MainCamera's transform.
        /// </summary>
        private Transform _camera;

        /// <summary>
        /// Cached metrics.
        /// </summary>
        private ValueMetric _frameTime;
        private ValueMetric _memory;
        
        public PerfMonitor PerfMonitor
        {
            get { return _monitor; }
        }

        public PerfMetricsCollector(
            IMetricsService metrics,
            IDeviceMetaProvider metaProvider,
            IPrimaryAnchorManager primaryAnchorManager,
            IBootstrapper bootstrapper, 
            RuntimeStats runtimeStats)
        {
            _metrics = metrics;
            _metaProvider = metaProvider;
            _primaryAnchorManager = primaryAnchorManager;
            _bootstrapper = bootstrapper;
            _runtimeStats = runtimeStats;

            var mainCam = Camera.main;
            if (!mainCam)
            {
                Log.Error(this, "No MainCamera yet?!");
            }
            else
            {
                _camera = mainCam.gameObject.transform;
            }
        }

        /// <summary>
        /// Starts the object.
        /// </summary>
        /// <param name="metrics">The metrics object.</param>
        public void Initialize(IMetricsService metrics)
        {
            _metrics = metrics;
            _monitor = new GameObject("PerfMonitor").AddComponent<PerfMonitor>();

            _frameTime = _metrics.Value(MetricsKeys.PERF_FRAMETIME);
            _memory = _metrics.Value(MetricsKeys.PERF_MEMORY);

            _bootstrapper.BootstrapCoroutine(TakeSnapshot());
        }

        /// <summary>
        /// Takes a snapshot and sends to graphite.
        /// </summary>
        /// <returns></returns>
        private IEnumerator TakeSnapshot()
        {
            // TODO: this is dumb, should wait until play mode
            yield return new WaitForSecondsRealtime(10f);

            while (true)
            {
                _frameTime.Value(_monitor.FrameTime.AverageMs);
                _memory.Value(_monitor.Memory.Allocated);

                _runtimeStats.Device.AllocatedMemory = _monitor.Memory.Allocated;
                _runtimeStats.Device.ReservedMemory = _monitor.Memory.Total;
                _runtimeStats.Device.MonoMemory = _monitor.Memory.Mono;
                _runtimeStats.Device.GpuMemory = _monitor.Memory.Gpu;
                _runtimeStats.Device.GraphicsDriverMemory = _monitor.Memory.GraphicsDriver;
                _runtimeStats.Device.Battery = _metaProvider.Meta().Battery;

                _runtimeStats.Camera.Position = _camera.position;
                _runtimeStats.Camera.Rotation = _camera.rotation;
                
                

                yield return new WaitForSecondsRealtime(5f);
            }
        }
    }
}