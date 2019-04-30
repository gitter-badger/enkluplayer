using System;
using System.Linq;
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
        private static bool _Initialized;

        /// <summary>
        /// Initializes the object with everything is needs.
        /// </summary>
        public static void Initialize(
            IDeviceMetaProvider deviceMetaProvider,
            IImageCapture imageCapture,
            IVideoCapture videoCapture,
            IMessageRouter msgRouter,
            IAppSceneManager sceneManager,
            IBootstrapper bootstrapper,
            AwsPingController awsPingController,
            ApiController apiController,
            ApplicationConfig config)
        {
            if (_Initialized)
            {
                throw new Exception("Dependencies already set!");
            }
            
            Instance.device = new DeviceJsApi(deviceMetaProvider, imageCapture, videoCapture);
            Instance.experiences = new ExperienceJsApi(msgRouter, bootstrapper, apiController, config);
            Instance.network = new NetworkJsApi(awsPingController);
            Instance.debugRendering = new DebugRenderingJsApi();
            Instance._sceneManager = sceneManager;

            _Initialized = true;
        }

        /// <summary>
        /// For modifying the root's schema.
        /// </summary>
        private IAppSceneManager _sceneManager;

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
        /// Provides API for debug rendering.
        /// </summary>
        public DebugRenderingJsApi debugRendering { get; private set; }

        /// <summary>
        /// Allows a custom message to display when the device is locating anchors.
        /// </summary>
        public string locatingMessage
        {
            get
            {
                var sceneId = _sceneManager.All.FirstOrDefault();
                if (string.IsNullOrEmpty(sceneId))
                {
                    Log.Error(this, "No scene found.");
                    return string.Empty;
                }
                
                var root = _sceneManager.Root(sceneId);
                var schema = root.Schema.Get<string>(AnchorManager.PROP_LOCATING_MESSAGE_KEY);
                return schema.Value;
            }
            set 
            { 
                var sceneId = _sceneManager.All.FirstOrDefault();
                if (string.IsNullOrEmpty(sceneId))
                {
                    Log.Error(this, "No scene found.");
                    return;
                }
                
                var root = _sceneManager.Root(sceneId);
                var schema = root.Schema.Get<string>(AnchorManager.PROP_LOCATING_MESSAGE_KEY);
                schema.Value = value;
            }
        }

        /// <summary>
        /// Allows anchor bypassing to be disabled.
        /// </summary>
        public bool disableAnchorBypass
        {
            get
            {
                var sceneId = _sceneManager.All.FirstOrDefault();
                if (string.IsNullOrEmpty(sceneId))
                {
                    Log.Error(this, "No scene found.");
                    return false;
                }
                
                var root = _sceneManager.Root(sceneId);
                var schema = root.Schema.Get<bool>(AnchorManager.PROP_DISABLE_BYPASS_KEY);
                return schema.Value;
            }
            set 
            { 
                var sceneId = _sceneManager.All.FirstOrDefault();
                if (string.IsNullOrEmpty(sceneId))
                {
                    Log.Error(this, "No scene found.");
                    return;
                }
                
                var root = _sceneManager.Root(sceneId);
                var schema = root.Schema.Get<bool>(AnchorManager.PROP_DISABLE_BYPASS_KEY);
                schema.Value = value;
            }
        }

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