namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Standard implementation.
    /// </summary>
    public class TcpConnectionFactory : ITcpConnectionFactory
    {
        /// <inheritdoc />
        public ITcpConnection Connection(ISocketListener listener)
        {
            return new TcpConnection(
                new LengthBasedSocketMessageReader(listener),
                new LengthBasedSocketMessageWriter());
        }
    }
}