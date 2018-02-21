using System.Net.NetworkInformation;
using System.Text;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Utils for networking.
    /// </summary>
    public static class NetworkUtils
    {
        /// <summary>
        /// Returns summary of network information.
        /// </summary>
        public static string GetNetworkSummary()
        {
            var builder = new StringBuilder();

#if UNITY_EDITOR || UNITY_IOS
            builder.AppendLine("Networking information:");
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                    || nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var ip in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            builder.AppendFormat("\t{0}\n", ip.Address);
                        }
                    }
                }
            }
#endif

#if NETFX_CORE
            builder.AppendLine("Networking information:");
            foreach (var localHostName in Windows.Networking.Connectivity.NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null)
                {
                    if (localHostName.Type == Windows.Networking.HostNameType.Ipv4)
                    {
                        builder.AppendFormat("IP : {0}\n", localHostName.ToString());
                    }
                }
            }
#endif
            
            return builder.ToString();
        }
    }
}
