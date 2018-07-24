namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Metric that tracks a value.
    /// </summary>
    public class ValueMetric
    {
        /// <summary>
        /// The target to send value to.
        /// </summary>
        private readonly IMetricsTarget _target;

        /// <summary>
        /// The key associated with this metric.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ValueMetric(IMetricsTarget target, string key)
        {
            _target = target;

            Key = key;
        }

        /// <summary>
        /// Sets the value of the metric.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Value(float value)
        {
            _target.Send(Key, value);
        }
    }
}