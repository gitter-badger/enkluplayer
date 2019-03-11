namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for a network stream.
    /// </summary>
    public interface INetworkStream
    {
        /// <summary>
        /// Writes a buffer to a stream.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset to start from in the buffer.</param>
        /// <param name="len">The number of bytes to write.</param>
        void Write(byte[] buffer, int offset, int len);
    }
}