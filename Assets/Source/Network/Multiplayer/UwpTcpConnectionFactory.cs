#if NETFX_CORE
namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Uwp implementation.
    /// </summary>
    public class UwpTcpConnectionFactory : ITcpConnectionFactory
    {
        /// <summary>
        /// Metrics service.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UwpTcpConnectionFactory(IMetricsService metrics) 
        {
            _metrics = metrics;
        }

        /// <inheritdoc />
        public ITcpConnection Connection(ISocketListener listener)
        {
            return new UwpTcpConnection(
                _metrics,
                listener,
                new LengthBasedSocketMessageWriter());
        }
    }
}
#endif