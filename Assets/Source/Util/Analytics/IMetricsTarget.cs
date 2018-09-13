namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes a target for metrics.
    /// </summary>
    public interface IMetricsTarget
    {
        /// <summary>
        /// Sends a metric.
        /// </summary>
        /// <param name="key">The appKey.</param>
        /// <param name="value">The value.</param>
        void Send(string key, float value);
    }
}