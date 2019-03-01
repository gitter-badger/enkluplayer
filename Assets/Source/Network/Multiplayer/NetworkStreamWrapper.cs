#if !NETFX_CORE
using System.Net.Sockets;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Wraps a stream.
    /// </summary>
    public class NetworkStreamWrapper : INetworkStream
    {
        /// <summary>
        /// Gets/sets the underlying stream.
        /// </summary>
        public NetworkStream Stream { get; set; }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int len)
        {
            if (null != Stream)
            {
                Stream.Write(buffer, offset, len);
            }
            else
            {
                Log.Warning(this, "Cannot write to a NetworkStreamWrapper with no NetworkStream.");
            }
        }
    }
}
#endif