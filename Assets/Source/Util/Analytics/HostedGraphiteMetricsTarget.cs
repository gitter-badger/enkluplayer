using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using CreateAR.Commons.Unity.Logging;

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
        /// Hostname.
        /// </summary>
        private readonly string _hostname;

        /// <summary>
        /// The socket.
        /// </summary>
        //private Socket _socket;
#if NETFX_CORE
        private Windows.Networking.Sockets.DatagramSocket _socket;

        private Windows.Storage.Streams.DataWriter _writer;
#else
        private System.Net.Sockets.Socket _socket;
#endif

        /// <summary>
        /// Constructor.
        /// </summary>
        public HostedGraphiteMetricsTarget(string hostname, string appKey)
        {
            if (string.IsNullOrEmpty(hostname))
            {
                throw new ArgumentNullException("hostname");
            }

            if (string.IsNullOrEmpty(appKey))
            {
                throw new ArgumentNullException("appKey");
            }

            _appKey = appKey;
            _hostname = hostname;

#if NETFX_CORE
            _socket = new Windows.Networking.Sockets.DatagramSocket();

            Connect();
#else
            var addresses = Dns.GetHostAddresses(hostname);
            if (addresses.Length > 0)
            {
                _endpoint = new IPEndPoint(addresses[0], 2003);
                _socket =
 new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp)
                {
                    Blocking = false
                };
            }
#endif
        }
        
        /// <inheritdoc />
        public void Send(string key, float value)
        {
            var message = string.Format("{0}.{1} {2}\n", _appKey, key,
                value);
            var bytes = Encoding.ASCII.GetBytes(message);

            Verbose(message);

#if NETFX_CORE
            Send(bytes);
#else
            _socket.SendTo(bytes, _endpoint);
#endif
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

#if NETFX_CORE
        /// <summary>
        /// Points the UWP socket.
        /// </summary>
        private async void Connect()
        {
            await _socket.ConnectAsync(new Windows.Networking.HostName(_hostname), "2003");

            _writer = new Windows.Storage.Streams.DataWriter(_socket.OutputStream);
        }

        /// <summary>
        /// Sends data.
        /// </summary>
        /// <param name="bytes">The bytes to send.</param>
        private async void Send(byte[] bytes)
        {
            _writer.WriteBytes(bytes);

            await _writer.StoreAsync();
        }
#endif

        /// <summary>
        /// Verbose logging.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}