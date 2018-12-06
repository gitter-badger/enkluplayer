#if !NETFX_CORE
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Sends metrics to hosted graphite.
    /// </summary>
    public class FrameworkHostedGraphiteMetricsTarget : IHostedGraphiteMetricsTarget, IDisposable
    {
        /// <summary>
        /// Type used in config.
        /// </summary>
        public const string TYPE = "HostedGraphite";

        /// <summary>
        /// Application appKey.
        /// </summary>
        private string _appKey;

        /// <summary>
        /// UDP endpoint.
        /// </summary>
        private IPEndPoint _endpoint;
        
        /// <summary>
        /// The socket.
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Constructor.
        /// </summary>
        public void Setup(string hostname, string appKey)
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
            
            try
            {
                var addresses = Dns.GetHostAddresses(hostname);
                if (addresses.Length > 0)
                {
                    _endpoint = new IPEndPoint(addresses[0], 2003);
                    _socket =
                        new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                        {
                            Blocking = false
                        };
                }
            }
            catch (Exception ex)
            {
                Log.Error(this, "Could not create socket : {0}", ex);
            }
        }

        /// <inheritdoc />
        public void Send(string key, float value)
        {
            if (null == _socket)
            {
                return;
            }

            var message = string.Format("{0}.{1} {2}\n", _appKey, key,
                value);
            var bytes = Encoding.ASCII.GetBytes(message);
            
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
        ~FrameworkHostedGraphiteMetricsTarget()
        {
            ReleaseUnmanagedResources();
        }
    }
}
#endif