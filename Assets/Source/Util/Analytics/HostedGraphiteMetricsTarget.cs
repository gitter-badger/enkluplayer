using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Sends metrics to hosted graphite.
    /// </summary>
    public class HostedGraphiteMetricsTarget : IMetricsTarget, IDisposable
    {
        /// <summary>
        /// Application appKey.
        /// </summary>
        private readonly string _appKey;

        /// <summary>
        /// UDP endpoint.
        /// </summary>
        private readonly IPEndPoint _endpoint;

        /// <summary>
        /// The socket.
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HostedGraphiteMetricsTarget(string hostname, string appKey)
        {
            _endpoint = new IPEndPoint(Dns.GetHostAddresses(hostname)[0], 2003);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                Blocking = false
            };
            _appKey = appKey;
        }

        /// <inheritdoc />
        public void Send(string key, float value)
        {
            var bytes = Encoding.ASCII.GetBytes(string.Format(
                "{0}.{1} {2}\n",
                _appKey,
                key,
                value));

            _socket.SendTo(bytes, _endpoint);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        private void ReleaseUnmanagedResources()
        {
            if (null != _socket)
            {
                _socket.Dispose();
                _socket = null;
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~HostedGraphiteMetricsTarget()
        {
            ReleaseUnmanagedResources();
        }
    }
}