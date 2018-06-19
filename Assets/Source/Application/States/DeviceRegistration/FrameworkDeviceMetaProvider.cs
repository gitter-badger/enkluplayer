﻿using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Implementation for .NETFramework.
    /// </summary>
    public class FrameworkDeviceMetaProvider : IDeviceMetaProvider
    {
        /// <inheritdoc />
        public DeviceResourceMeta Meta()
        {
            return new DeviceResourceMeta
            {
                Battery = 0.75f,
                EnkluVersion = "Unknown",
                Ips = GetIps(),
                UwpVersion = "Unknown"
            };
        }

        /// <summary>
        /// Retrieves all IPs.
        /// </summary>
        /// <returns></returns>
        private string[] GetIps()
        {
            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => ip.ToString())
                .ToArray();
        }
    }
}