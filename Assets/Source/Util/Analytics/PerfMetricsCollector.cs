using System.Collections;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Forwards performance metrics to graphite.
    /// </summary>
    public class PerfMetricsCollector : MonoBehaviour
    {
        /// <summary>
        /// Metrics.
        /// </summary>
        private IMetricsService _metrics;

        /// <summary>
        /// Monitors metrics.
        /// </summary>
        private PerfMonitor _monitor;

        /// <summary>
        /// Cached metrics.
        /// </summary>
        private ValueMetric _frameTime;
        private ValueMetric _memory;

        /// <summary>
        /// Starts the object.
        /// </summary>
        /// <param name="metrics">The metrics object.</param>
        /// <param name="monitor">The monitor to use.</param>
        public void Initialize(IMetricsService metrics, PerfMonitor monitor)
        {
            _metrics = metrics;
            _monitor = monitor;

            _frameTime = _metrics.Value(MetricsKeys.PERF_FRAMETIME);
            _memory = _metrics.Value(MetricsKeys.PERF_MEMORY);

            StartCoroutine(TakeSnapshot());
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