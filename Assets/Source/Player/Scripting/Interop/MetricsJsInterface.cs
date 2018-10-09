using System.Collections.Generic;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for metrics.
    /// </summary>
    [JsInterface("metrics")]
    public class MetricsJsInterface
    {
        /// <summary>
        /// Metrics object.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Parallel lists from timer id to timer object.
        /// </summary>
        private readonly List<int> _timerIds = new List<int>();
        private readonly List<TimerMetric> _timers = new List<TimerMetric>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MetricsJsInterface(IMetricsService metrics)
        {
            _metrics = metrics;
        }

        /// <summary>
        /// Starts a timer for a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Unique int for this start invocation.</returns>
        public int startTimer(string key)
        {
            var timer = _metrics.Timer(key);
            var id = timer.Start();

            // add at the beginning as timers are most likely hierarchical
            // this is also helpful if a caller never calls stop or abort
            _timerIds.Insert(0, id);
            _timers.Insert(0, timer);

            return id;
        }

        /// <summary>
        /// Stops a timer for a specific invocation.
        /// </summary>
        /// <param name="id">The invocation id.</param>
        public void stopTimer(int id)
        {
            var index = _timerIds.IndexOf(id);
            if (-1 != index)
            {
                _timers[index].Stop(id);

                _timerIds.RemoveAt(index);
                _timers.RemoveAt(index);
            }
        }

        /// <summary>
        /// Aborsts a timer for a specific invocation.
        /// </summary>
        /// <param name="id">The invocation id.</param>
        public void abortTimer(int id)
        {
            var index = _timerIds.IndexOf(id);
            if (-1 != index)
            {
                _timers[index].Abort(id);

                _timerIds.RemoveAt(index);
                _timers.RemoveAt(index);
            }
        }

        /// <summary>
        /// Increments a counter for a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void incrementCounter(string key)
        {
            _metrics.Counter(key).Increment();
        }

        /// <summary>
        /// Decrements a counter for a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void decrementCounter(string key)
        {
            _metrics.Counter(key).Decrement();
        }

        /// <summary>
        /// Sets a value for a key directly.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value to set.</param>
        public void setValue(string key, float value)
        {
            _metrics.Value(key).Value(value);
        }
    }
}