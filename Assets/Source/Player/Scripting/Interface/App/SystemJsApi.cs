using System;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// System-wide API.
    /// </summary>
    public class SystemJsApi
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static SystemJsApi Instance = new SystemJsApi();

        /// <summary>
        /// Provider for getting DeviceMeta.
        /// </summary>
        public static IDeviceMetaProvider DeviceMetaProvider
        {
            set
            {
                if (Instance.device != null)
                {
                    throw new Exception("DeviceMetaProvider already configured");
                }

                Instance.device = new DeviceJsApi(value);
            }
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

        /// <summary>
        /// Terminates the application.
        /// </summary>
        public void terminate()
        {
#if UNITY_WSA
            Log.Info(this, "Terminating.");
            UnityEngine.Application.Quit();
#else
            Log.Warning(this, "Terminate not supported for this platform");
#endif
        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        public void restart()
        {

#if NETFX_CORE
            Log.Info(this, "Restarting.");
            Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync("");
#else
            Log.Warning(this, "Restart not supported for this platform.");
#endif
        }
    }
}