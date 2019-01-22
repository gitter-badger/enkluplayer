using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Provides scripting access to some of the device's meta.
    /// </summary>
    public class DeviceJsApi
    {
        /// <summary>
        /// Provider. Used to update whenever the battery level is queried.
        /// </summary>
        private IDeviceMetaProvider _deviceMeta;

        /// <summary>
        /// Cached meta for non-battery requests.
        /// </summary>
        private DeviceResourceMeta _meta;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="deviceMeta"></param>
        public DeviceJsApi(IDeviceMetaProvider deviceMeta)
        {
            _deviceMeta = deviceMeta;
            _meta = _deviceMeta.Meta();
        }

        /// <summary>
        /// Enklu version string.
        /// </summary>
        public string enkluVersion
        {
            get { return _meta.EnkluVersion; }
        }

        /// <summary>
        /// UWP version string.
        /// </summary>
        public string uwpVersion
        {
            get { return _meta.UwpVersion; }
        }

        /// <summary>
        /// Battery %. Refreshed every access, so cache as much as possible.
        /// </summary>
        public float battery
        {
            get
            {
                // Update the meta, since battery is cached in meta.
                _meta = _deviceMeta.Meta();
                return _meta.Battery;
            }
        }

        /// <summary>
        /// Unique hardware id.
        /// </summary>
        public string hardwareId
        {
            get { return SystemInfo.deviceUniqueIdentifier; }
        }
    }
}