#if NETFX_CORE
namespace CreateAR.SpirePlayer
{
    public class NetCoreDeviceMetaProvider : IDeviceMetaProvider
    {
        public DeviceResourceMeta Meta()
        {
            var deviceFamilyVersion = Window.SystemProfile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong version = ulong.Parse(deviceFamilyVersion);
            ulong major = (version & 0xFFFF000000000000L) >> 48;
            ulong minor = (version & 0x0000FFFF00000000L) >> 32;
            ulong build = (version & 0x00000000FFFF0000L) >> 16;
            ulong revision = (version & 0x000000000000FFFFL);
            var osVersion = $"{major}.{minor}.{build}.{revision}";

            var aggBattery = Windows.Devices.Power.Battery.AggregateBattery; 
            var report = aggBattery.GetReport();

            var package = Windows.ApplicationModel.Package.Current;
            var version = package.Id.Version.ToString();

            return new DeviceResourceMeta
            {
                UwpVersion = deviceFamilyVersion,
                Battery = report.RemainingCapacityInMilliwattHours / report.FullChargeCapacityInMilliwattHours,
                EnkluVersion = package,
                Ips = GetIps()
            };
        }

        public string[] GetIps()
        {
            return Windows.Networking.Connectivity.NetworkInformation
                .GetHostNames()
                .Where(ipa => ip.AddressFamily == AddressFamily.InterNetwork)
                .Select(ipa => ipa.ToString())
                .ToArray();
        }
    }
}
#endif