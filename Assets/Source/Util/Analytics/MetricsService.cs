using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Basic implementation of <c>IMetricsService</c>.
    /// </summary>
    public class MetricsService : IMetricsService
    {
        /// <summary>
        /// Tracks all metric targets.
        /// </summary>
        private readonly AggregateMetricsTarget _target = new AggregateMetricsTarget();

        /// <summary>
        /// Collection of timers.
        /// </summary>
        private readonly List<TimerMetric> _timers = new List<TimerMetric>();

        /// <summary>
        /// Collection of counters.
        /// </summary>
        private readonly List<CounterMetric> _counters = new List<CounterMetric>();

        /// <summary>
        /// Collection of values.
        /// </summary>
        private readonly List<ValueMetric> _values = new List<ValueMetric>();

        /// <inheritdoc />
        public void AddTarget(IMetricsTarget target)
        {
            _target.Add(target);
        }

        /// <inheritdoc />
        public TimerMetric Timer(string key)
        {
            for (int i = 0, len = _timers.Count; i < len; i++)
            {
                var timer = _timers[i];
                if (timer.Key == key)
                {
                    return timer;
                }
            }

            var newTimer = new TimerMetric(_target, key);
            _timers.Add(newTimer);

            return newTimer;
        }

        /// <inheritdoc />
        public CounterMetric Counter(string key)
        {
            for (int i = 0, len = _counters.Count; i < len; i++)
            {
                var counter = _counters[i];
                if (counter.Key == key)
                {
                    return counter;
                }
            }

            var newCounter = new CounterMetric(_target, key);
            _counters.Add(newCounter);

            return newCounter;
        }

        /// <inheritdoc />
        public ValueMetric Value(string key)
        {
            for (int i = 0, len = _values.Count; i < len; i++)
            {
                var value = _values[i];
                if (value.Key == key)
                {
                    return value;
                }
            }

            var newValue = new ValueMetric(_target, key);
            _values.Add(newValue);

            return newValue;
        }
    }
}