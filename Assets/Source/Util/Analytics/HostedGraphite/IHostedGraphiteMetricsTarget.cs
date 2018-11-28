namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Named metrics target.
    /// </summary>
    public interface IHostedGraphiteMetricsTarget : IMetricsTarget
    {
        /// <summary>
        /// Prepares the target for use.
        /// </summary>
        /// <param name="hostname">The hostname.</param>
        /// <param name="key">The API key.</param>
        void Setup(string hostname, string key);
    }
}