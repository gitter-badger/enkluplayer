#if !NETFX_CORE
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Implementation for .NETFramework.
    /// </summary>
    public class FrameworkDeviceMetaProvider : IDeviceMetaProvider
    {
        /// <summary>
        /// PRNG.
        /// </summary>
        private readonly Random _random = new Random();

        /// <inheritdoc />
        public DeviceResourceMeta Meta()
        {
            return new DeviceResourceMeta
            {
                Battery = (float) _random.NextDouble(),
                EnkluVersion = "Editor",
                Ips = GetIps(),
                UwpVersion = "Editor"
            };
        }

        /// <summary>
        /// Retrieves all IPs.
        /// </summary>
        /// <returns></returns>
        private string[] GetIps()
        {
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ip => ip.ToString())
                    .ToArray();
            }
            catch
            {
                return new string[0];
            }
        }
    }
}
#endif