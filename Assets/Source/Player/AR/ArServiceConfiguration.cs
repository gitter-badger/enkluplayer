using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.EnkluPlayer.AR
{
    /// <summary>
    /// Configures an <c>IArService</c> implementation.
    /// </summary>
    public class ArServiceConfiguration : MonoBehaviour
    {
        /// <summary>
        /// True iff the camera feed should be rendered behind everything.
        /// </summary>
        public bool ShowCameraFeed;
        
        /// <summary>
        /// True iff planes should be detected.
        /// </summary>
        public bool EnablePlaneDetection;
        
        /// <summary>
        /// True iff lighting estimations should be taken into account.
        /// </summary>
        public bool EnableLightEstimation;
        
        /// <summary>
        /// True iff point cloud should be detected.
        /// </summary>
        public bool EnablePointCloud;
        
        /// <summary>
        /// True iff we should draw planes.
        /// </summary>
        public bool DrawPlanes;

        /// <summary>
        /// The camera rig.
        /// </summary>
        public ArCameraRig Rig;

        /// <summary>
        /// Material for rendering camera feed.
        /// </summary>
        public List<PlatformSpecificSettings> PlatformSpecificSettings;

        /// <summary>
        /// Minimum seconds to search.
        /// </summary>
        public int MinSearchSec = 3;

        /// <summary>
        /// Maxiumum seconds to search.
        /// </summary>
        public int MaxSearchSec = 20;

        /// <summary>
        /// Helper method to retrieve the appropriate settings for a platform
        /// </summary>
        /// <param name="platform">The platform for which to retrieve the session settings</param>
        /// <returns>The session-specific settings for our platform</returns>
        public PlatformSpecificSettings GetSettings(RuntimePlatform platform)
        {
            for (int i = 0; i < PlatformSpecificSettings.Count; i++)
            {
                if (PlatformSpecificSettings[i].Platform == platform)
                {
                    return PlatformSpecificSettings[i];
                }
            }

            Commons.Unity.Logging.Log.Error(this, "Failed to find camera material for platform: " + platform);
            return null;
        }
    }

    [Serializable]
    public class PlatformSpecificSettings
    {
        public RuntimePlatform Platform;
        public Material CameraMaterial;
        public GameObject SessionPrefab;
    }
}
