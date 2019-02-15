using System.Collections;
using CreateAR.Commons.Unity.Http;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Forwards performance metrics to graphite.
    /// </summary>
    public class PerfMetricsCollector
    {
        /// <summary>
        /// Cached metrics.
        /// </summary>
        private readonly ValueMetric _frameTime;
        private readonly ValueMetric _memory;

        /// <summary>
        /// The underlying performance monitor.
        /// </summary>
        public PerfMonitor PerfMonitor { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PerfMetricsCollector(
            IMetricsService metrics,
            IBootstrapper bootstrapper)
        {
            PerfMonitor = new GameObject("PerfMonitor").AddComponent<PerfMonitor>();

            // get references to the metrics
            _frameTime = metrics.Value(MetricsKeys.PERF_FRAMETIME);
            _memory = metrics.Value(MetricsKeys.PERF_MEMORY);

            bootstrapper.BootstrapCoroutine(TakeSnapshot());
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
                _frameTime.Value(PerfMonitor.FrameTime.AverageMs);
                _memory.Value(PerfMonitor.Memory.Allocated);

                yield return new WaitForSecondsRealtime(5f);
            }
        }
    }
}