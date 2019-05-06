using System;

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
}