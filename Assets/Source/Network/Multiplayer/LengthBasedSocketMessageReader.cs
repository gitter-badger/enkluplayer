using System;
using System.IO;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// This delegate is used to pass a read message from the socket message reader to the messaging system for deserialization.
    /// It is called on the socket read thread, so the <c>messageBytes</c> data must be handled synchronously or copied. 
    /// </summary>
    public delegate void SocketMessageRead(ArraySegment<byte> messageBytes);

    /// <summary>
    /// This <see cref="ISocketMessageReader"/> implementation reads a leading N-byte length encoding to determine the amount of data
    /// to aggregate from the socket buffer before dispatching a message.
    /// </summary>
    public class LengthBasedSocketMessageReader : ISocketMessageReader
    {
        /// <summary>
        /// Internal message buffer that will be used to aggregate the socket data until a full message
        /// is assembled.
        /// </summary>
        private readonly MemoryStream _messageStream = new MemoryStream();

        /// <summary>
        /// The length of the current message being collected.
        /// </summary>
        private long _currentMessageLength = 0;

        /// <summary>
        /// The size of the length field, in bytes.
        /// </summary>
        private readonly int _lengthSize;

        /// <summary>
        /// A byte buffer to use to parse the length values from the socket buffer.
        /// </summary>
        private readonly byte[] _lengthBuffer;
    
        /// <summary>
        /// Delegate used to pipe the resulting buffers 
        /// </summary>
        private readonly SocketMessageRead _onMessageRead;

        /// <summary>
        /// Creates a new <see cref="LengthBasedSocketMessageReader"/> instance.
        /// </summary>
        /// <param name="lengthSize">The size, in bytes, of the prepended length data.</param>
        /// <param name="onMessageRead">The delegate to execute once a full message has been read.</param>
        public LengthBasedSocketMessageReader(int lengthSize, SocketMessageRead onMessageRead)
        {
            if (!LengthFieldHelper.IsLegalLengthSize(lengthSize))
            {
                throw new ArgumentException("Length of the message length should be 1, 2, 4, or 8.");
            }

            _lengthSize = lengthSize;
            _lengthBuffer = new byte[lengthSize];

            _onMessageRead = onMessageRead;
        }

        /// <inheritdoc/>
        public void DataRead(MemoryStream stream)
        {
            // Parse as many messages from the incoming stream as possible
            while (stream.GetBytesRemaining() > 0)
            {
                // The message length is always the first N bytes in the stream
                if (_messageStream.Length == 0 && stream.GetBytesRemaining() >= _lengthSize)
                {
                    stream.Read(_lengthBuffer, 0, _lengthSize);
                    _currentMessageLength = LengthFieldHelper.GetFrameLength(_lengthBuffer, _lengthSize);
                }
            
                // Enough bytes to extract a message
                if (stream.GetBytesRemaining() >= _currentMessageLength)
                {
                    stream.ReadBytes(_messageStream, _messageStream.Length, _currentMessageLength);
                    _currentMessageLength = 0;
                }
                // Copy as much as possible into internal buffer
                else
                {
                    _currentMessageLength -= stream.GetBytesRemaining();
                    stream.ReadBytes(_messageStream, _messageStream.Length);
                }

                // There is a full message we can dispatch
                if (_currentMessageLength == 0)
                {
                    // On Message Read Handler must either copy or act on the data synchronously
                    _onMessageRead.Invoke(new ArraySegment<byte>(_messageStream.GetBuffer(), 0, (int)_messageStream.Length));
                    _messageStream.Clear();
                }
            }
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _currentMessageLength = 0;
            _messageStream.Clear();
        }
    }
}