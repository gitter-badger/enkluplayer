using System;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for a connection.
    /// </summary>
    public interface ITcpConnection
    {
        /// <summary>
        /// Whether or not the <see cref="TcpConnection"/> is connected to an endpoint.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Fired when the connection is closed. The <see cref="bool"/> flag is set to
        /// <c>true</c> when the connection has been dropped from the server.
        /// </summary>
        Action<bool> OnConnectionClosed { get; set; }

        /// <summary>
        /// Synchronously connects the the target host and port.
        /// </summary>
        bool Connect(string host, int port);

        /// <summary>
        /// Uses the <see cref="ISocketMessageWriter"/> to write data to the outbound TCP buffer.
        /// </summary>
        /// <param name="message">The binary payload to write to the outbound buffer.</param>
        /// <returns><c>true</c> if the write succeeded. Otherwise, <c>false</c></returns>
        bool Send(byte[] message, int offset, int len);

        /// <summary>
        /// Closes the TCP connection and stops the socket reading thread.
        /// </summary>
        void Close();
    }
}