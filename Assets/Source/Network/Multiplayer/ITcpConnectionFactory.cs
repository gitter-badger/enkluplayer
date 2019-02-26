namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that creates <c>ITcpConnection</c> instances.
    /// </summary>
    public interface ITcpConnectionFactory
    {
        /// <summary>
        /// Creates an <c>ITcpConnection</c> for a listener.
        /// </summary>
        /// <param name="listener">The listener that receives read events.</param>
        /// <returns></returns>
        ITcpConnection Connection(ISocketListener listener);
    }
}