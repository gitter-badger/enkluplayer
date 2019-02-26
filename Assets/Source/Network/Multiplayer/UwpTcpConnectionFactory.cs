#if NETFX_CORE
namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Uwp implementation.
    /// </summary>
    public class UwpTcpConnectionFactory : ITcpConnectionFactory
    {
        /// <inheritdoc />
        public ITcpConnection Connection(ISocketListener listener)
        {
            return new UwpTcpConnection(
                listener,
                new LengthBasedSocketMessageWriter());
        }
    }
}
#endif