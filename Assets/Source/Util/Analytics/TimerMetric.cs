using System;
using System.Collections.Generic;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Metric that times things.
    /// </summary>
    public class TimerMetric
    {
        /// <summary>
        /// The unit of measurement.
        /// </summary>
        public enum TimeUnit
        {
            Milliseconds,
            Seconds
        }

        /// <summary>
        /// Unique ids.
        /// </summary>
        private static int IDS;

        /// <summary>
        /// The target to send to.
        /// </summary>
        private readonly IMetricsTarget _target;
        
        /// <summary>
        /// Parallel lists of ids and start times.
        /// </summary>
        private readonly List<int> _ids = new List<int>();
        private readonly List<DateTime> _starts = new List<DateTime>();

        /// <summary>
        /// The key associated with this metric.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// The format in which to send the time spans.
        /// </summary>
        public TimeUnit Format { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimerMetric(IMetricsTarget target, string key)
        {
            _target = target;
            Key = key;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <returns>Returns a unique id associated with this call to Start().</returns>
        public int Start()
        {
            var id = IDS++;
            _ids.Add(id);
            _starts.Add(DateTime.Now);

            return id;
        }

        /// <summary>
        /// Stops a timer by id.
        /// </summary>
        /// <param name="id">The id of the timer.</param>
        public void Stop(int id)
        {
            var index = _ids.IndexOf(id);
            if (-1 == index)
            {
                return;
            }

            var start = _starts[index];

            _starts.RemoveAt(index);
            _ids.RemoveAt(index);

            var span = DateTime.Now.Subtract(start);

            _target.Send(
                Key,
                (float) (Format == TimeUnit.Milliseconds
                    ? span.TotalMilliseconds
                    : span.TotalSeconds));
        }

        /// <summary>
        /// Aborts a timer by id.
        /// </summary>
        /// <param name="id">The id of the timer.</param>
        public void Abort(int id)
        {
            var index = _ids.IndexOf(id);
            if (-1 == index)
            {
                return;
            }
            
            _starts.RemoveAt(index);
            _ids.RemoveAt(index);
        }
    }
}