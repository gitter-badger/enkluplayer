#if NETFX_CORE
using System.Linq;
using Windows.Networking;
using Windows.System.Profile;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Provider implementation for UWP devices.
    /// </summary>
    public class NetCoreDeviceMetaProvider : IDeviceMetaProvider
    {
        /// <inheritdoc />
        public DeviceResourceMeta Meta()
        {
            var deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            var version = ulong.Parse(deviceFamilyVersion);
            var major = (version & 0xFFFF000000000000L) >> 48;
            var minor = (version & 0x0000FFFF00000000L) >> 32;
            var build = (version & 0x00000000FFFF0000L) >> 16;
            var revision = (version & 0x000000000000FFFFL);
            var osVersion = $"{major}.{minor}.{build}.{revision}";

            var aggBattery = Windows.Devices.Power.Battery.AggregateBattery; 
            var report = aggBattery.GetReport();
            var remaining = report.RemainingCapacityInMilliwattHours ?? 0;
            var capacity = report.FullChargeCapacityInMilliwattHours ?? 1;

            var package = Windows.ApplicationModel.Package.Current;
            var packageVersion = package.Id.Version;

            return new DeviceResourceMeta
            {
                UwpVersion = osVersion,
                Battery = remaining / (float) capacity,
                EnkluVersion = $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}",
                Ips = GetIps()
            };
        }

        /// <summary>
        /// Retrieves list of IP addresses.
        /// </summary>
        /// <returns></returns>
        public string[] GetIps()
        {
            return Windows.Networking.Connectivity.NetworkInformation
                .GetHostNames()
                .Where(hostName => null != hostName.IPInformation)
                .Where(hostName => hostName.Type == HostNameType.Ipv4)
                .Select(ipa => ipa.ToString())
                .ToArray();
        }
    }
}
#endif