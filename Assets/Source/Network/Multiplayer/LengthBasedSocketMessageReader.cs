using System;
using System.IO;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that can listen to socket events.
    /// </summary>
    public interface ISocketListener
    {
        /// <summary>
        /// Called when the raw bytes of a message have been identified. This
        /// will be called on the read thread and assumes bytes are consumed
        /// synchronously.
        /// </summary>
        /// <param name="messageBytes">The bytes.</param>
        void HandleSocketMessage(ArraySegment<byte> messageBytes);
    }

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
        /// A byte buffer to use to parse the length values from the socket buffer.
        /// </summary>
        private readonly byte[] _lengthBuffer;
    
        /// <summary>
        /// Receives message payloads.
        /// </summary>
        private readonly ISocketListener _listener;

        /// <summary>
        /// Creates a new <see cref="LengthBasedSocketMessageReader"/> instance.
        /// </summary>
        public LengthBasedSocketMessageReader(ISocketListener listener)
        {
            _lengthBuffer = new byte[2];
            _listener = listener;
        }

        /// <inheritdoc/>
        public void DataRead(MemoryStream stream)
        {
            // Parse as many messages from the incoming stream as possible
            while (stream.GetBytesRemaining() > 0)
            {
                // The message length is always the first N bytes in the stream
                if (_messageStream.Length == 0 && stream.GetBytesRemaining() >= 2)
                {
                    stream.Read(_lengthBuffer, 0, 2);
                    _currentMessageLength = (ushort) HeapByteBufferUtil.GetShort(_lengthBuffer, 0);
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
                    _listener.HandleSocketMessage(new ArraySegment<byte>(_messageStream.GetBuffer(), 0, (int)_messageStream.Length));
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