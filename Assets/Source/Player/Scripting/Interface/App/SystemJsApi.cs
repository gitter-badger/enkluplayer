using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;

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
        public static readonly SystemJsApi Instance = new SystemJsApi();
        
        /// <summary>
        /// Guard to make sure this isn't configured twice.
        /// </summary>
        private static bool _initialized;

        public static void Initialize(
            IDeviceMetaProvider deviceMetaProvider,
            NetworkConnectivity networkConnectivity,
            IMessageRouter msgRouter,
            ApiController apiController,
            ApplicationConfig config)
        {
            if (_initialized)
            {
                throw new Exception("Dependencies already set!");
            }
            
            Instance.device = new DeviceJsApi(deviceMetaProvider);
            Instance.experiences = new ExperienceJsApi(msgRouter, apiController, config);
            Instance.network = new NetworkJsApi(networkConnectivity);

            _initialized = true;
        }

        /// <summary>
        /// Provides details about the device.
        /// </summary>
        public DeviceJsApi device { get; private set; }

        /// <summary>
        /// Provides API for experiences.
        /// </summary>
        public ExperienceJsApi experiences { get; private set; }
        
        /// <summary>
        /// Provides API for networking.
        /// </summary>
        public NetworkJsApi network { get; private set; }

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