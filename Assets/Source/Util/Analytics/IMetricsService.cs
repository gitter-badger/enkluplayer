namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Descibes a service for collecting metrics.
    /// </summary>
    public interface IMetricsService
    {
        /// <summary>
        /// Adds a metric target.
        /// </summary>
        /// <param name="target">The target to add.</param>
        void AddTarget(IMetricsTarget target);

        /// <summary>
        /// Retrieves a timer.
        /// </summary>
        /// <param name="key">The key to associate with this timer./</param>
        /// <returns></returns>
        TimerMetric Timer(string key);

        /// <summary>
        /// Retrieves a counter.
        /// </summary>
        /// <param name="key">The key to associate with this counter./</param>
        /// <returns></returns>
        CounterMetric Counter(string key);

        /// <summary>
        /// Retrieves a value.
        /// </summary>
        /// <param name="key">The key to associate with this value./</param>
        /// <returns></returns>
        ValueMetric Value(string key);
    }
}
