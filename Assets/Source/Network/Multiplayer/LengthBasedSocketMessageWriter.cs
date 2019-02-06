using System;
using System.IO;
using System.Net.Sockets;

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
        /// The size of the length field, in bytes.
        /// </summary>
        private readonly int _lengthSize;

        /// <summary>
        /// A byte buffer to use to parse the length values from the socket buffer.
        /// </summary>
        private readonly byte[] _lengthBuffer;

        /// <summary>
        /// Creates a new <see cref="LengthBasedSocketMessageWriter"/> instance.
        /// </summary>
        /// <param name="lengthSize">The number of bytes to use when prepending the length field.</param>
        public LengthBasedSocketMessageWriter(int lengthSize)
        {
            if (!LengthFieldHelper.IsLegalLengthSize(lengthSize))
            {
                throw new ArgumentException("Length of the message length should be 1, 2, 4, or 8.");
            }

            _lengthSize = lengthSize;
            _lengthBuffer = new byte[lengthSize];
            _writeStream = new MemoryStream();
        }

        /// <inheritdoc/>
        public void DataWrite(byte[] data, NetworkStream stream)
        {
            LengthFieldHelper.WriteLength(data.LongLength, _lengthBuffer, _lengthSize);

            // Reset the memory stream to the origin, write the length and payload
            _writeStream.Position = 0;
            _writeStream.Write(_lengthBuffer, 0, _lengthBuffer.Length);
            _writeStream.Write(data, 0, data.Length);

            // write the length + payload to the socket
            stream.Write(_writeStream.GetBuffer(), 0, data.Length + _lengthSize);
        }
    }
}