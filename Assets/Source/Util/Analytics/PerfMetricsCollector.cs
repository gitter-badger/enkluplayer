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
        private IBootstrapper _bootstrapper;

        /// <summary>
        /// Monitors metrics.
        /// </summary>
        private PerfMonitor _monitor;

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
            IBootstrapper bootstrapper)
        {
            _metrics = metrics;
            _bootstrapper = bootstrapper;

            _monitor = new GameObject("PerfMonitor").AddComponent<PerfMonitor>();
        }

        /// <summary>
        /// Starts the object.
        /// </summary>
        /// <param name="metrics">The metrics object.</param>
        public void Initialize(IMetricsService metrics)
        {
            _metrics = metrics;

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

                yield return new WaitForSecondsRealtime(5f);
            }
        }
    }
}