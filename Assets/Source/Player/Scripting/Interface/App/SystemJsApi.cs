namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// System-wide API.
    /// </summary>
    public class SystemJsApi
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="deviceMeta"></param>
        public SystemJsApi(IDeviceMetaProvider deviceMeta)
        {
            device = new DeviceJsApi(deviceMeta);
        }

        /// <summary>
        /// Provides details about the device.
        /// </summary>
        public DeviceJsApi device { get; private set; }

        /// <summary>
        /// Recenters tracking.
        /// </summary>
        public void recenter()
        {
#if NETFX_CORE
                UnityEngine.XR.InputTracking.Recenter();
#endif
        }
    }
}