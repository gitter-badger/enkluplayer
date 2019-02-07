using System.Net.Sockets;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// This implementation prototype defines an object capable of writing a binary payload to a network stream. 
    /// </summary>
    public interface ISocketMessageWriter
    {
        /// <summary>
        /// This method writes the binary payload to the <see cref="NetworkStream"/>.
        /// </summary>
        /// <param name="data">The binary data to write to the stream.</param>
        /// <param name="stream">The <see cref="NetworkStream"/> to write the data to.</param>
        /// <param name="offset">The offset into the data to start writing.</param>
        /// <param name="len">The length.</param>
        void Write(NetworkStream stream, byte[] data, int offset, int len);
    }
}