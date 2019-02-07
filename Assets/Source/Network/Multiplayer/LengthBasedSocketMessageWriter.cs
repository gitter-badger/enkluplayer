using System;
using System.IO;
using System.Net.Sockets;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// This class is used to prepend a payload length before writing the data to the socket. The destination
    /// will use the prepended length to parse the binary data. Internally, the writer uses an expandable <see cref="MemoryStream"/>
    /// to pool memory such that reallocation only happens on expansion. 
    /// </summary>
    public class LengthBasedSocketMessageWriter : ISocketMessageWriter
    {
        /// <summary>
        /// The internal write buffer used to aggregate the payload before writing to the network stream.
        /// </summary>
        private readonly MemoryStream _writeStream;

        /// <summary>
        /// Creates a new <see cref="LengthBasedSocketMessageWriter"/> instance.
        /// </summary>
        public LengthBasedSocketMessageWriter()
        {
            _writeStream = new MemoryStream();
        }

        /// <inheritdoc/>
        public void Write(NetworkStream stream, byte[] data, int offset, int len)
        {
            // Reset the memory stream to the origin, write the length and payload
            _writeStream.Position = 0;
            
            // write length first
            _writeStream.WriteByte((byte)((ushort) len >> 8));
            _writeStream.WriteByte((byte) len);
            
            // write data
            _writeStream.Write(data, offset, len);

            // write the length + payload to the socket
            stream.Write(_writeStream.GetBuffer(), 0, len + 2 /* length */);
        }
    }
}