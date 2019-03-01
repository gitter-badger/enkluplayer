using System.IO;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// This implementation prototype defines an object capable of accepting a position aware stream, and using the available data to
    /// read a specific implementation of a binary protocol.
    /// </summary>
    public interface ISocketMessageReader
    {
        /// <summary>
        /// This method is called using a live memory stream from the socket read thread. Any unread data will carry over into the next read
        /// call. This method will always be called from the same thread reading the socket, so operations must be synchronous unless copied.
        /// </summary>
        /// <param name="readStream">The stream the reader will accept as input.</param>
        void DataRead(MemoryStream readStream);

        /// <summary>
        /// Resets any aggregate data collected during the read.
        /// </summary>
        void Reset();
    }
}