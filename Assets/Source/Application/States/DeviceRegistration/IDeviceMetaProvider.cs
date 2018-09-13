namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that can return meta about the device.
    /// </summary>
    public interface IDeviceMetaProvider
    {
        /// <summary>
        /// Provides meta about a device.
        /// </summary>
        /// <returns></returns>
        DeviceResourceMeta Meta();
    }
}