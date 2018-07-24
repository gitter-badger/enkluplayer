namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Integral metric for counting things.
    /// </summary>
    public class CounterMetric
    {
        /// <summary>
        /// The target to send value to.
        /// </summary>
        private readonly IMetricsTarget _target;

        /// <summary>
        /// The key of the counter.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// The value of the counter.
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CounterMetric(IMetricsTarget target, string key)
        {
            _target = target;

            Key = key;
            Value = 0;
        }

        /// <summary>
        /// Adds to the value.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void Add(int value)
        {
            Value += value;

            _target.Send(Key, Value);
        }

        /// <summary>
        /// Subtracts from the counter.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Subtract(int value)
        {
            Add(-value);
        }

        /// <summary>
        /// Increments the value.
        /// </summary>
        public void Increment()
        {
            Add(1);
        }

        /// <summary>
        /// Decrements the value.
        /// </summary>
        public void Decrement()
        {
            Subtract(1);
        }

        /// <summary>
        /// Sets the counter to a specific value directly.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Count(int value)
        {
            Value = value;

            _target.Send(Key, value);
        }
    }
}