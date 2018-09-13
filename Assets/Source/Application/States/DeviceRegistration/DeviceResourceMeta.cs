namespace CreateAR.EnkluPlayer
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

        /// <summary>
        /// ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[DeviceResourceMeta UwpVersion={0}, EnkluVersion={1}, Battery={2}, Ips={3}]",
                UwpVersion,
                EnkluVersion,
                Battery,
                string.Join(", ", Ips));
        }
    }
}