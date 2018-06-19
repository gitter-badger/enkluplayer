namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Meta information about a device.
    /// </summary>
    public class DeviceResourceMeta
    {
        /// <summary>
        /// List of IP addresses.
        /// </summary>
        public string[] Ips;

        /// <summary>
        /// Battery life.
        /// </summary>
        public float Battery;

        /// <summary>
        /// Enklu version.
        /// </summary>
        public string EnkluVersion;

        /// <summary>
        /// Uwp version.
        /// </summary>
        public string UwpVersion;
    }
}