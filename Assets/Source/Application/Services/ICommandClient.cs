using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes an object from which a command originates.
    /// </summary>
    public interface ICommandClient
    {
        /// <summary>
        /// Called when a command client has been closed.
        /// </summary>
        event Action<ICommandClient> OnClosed;

        /// <summary>
        /// Sends a message to client that sent the message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        void Send(string message);
    }
}