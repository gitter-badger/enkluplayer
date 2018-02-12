using UnityEngine;

namespace CreateAR.SpirePlayer.AR
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
        public Material CameraMaterial;

        /// <summary>
        /// Minimum seconds to search.
        /// </summary>
        public int MinSearchSec = 3;

        /// <summary>
        /// Maxiumum seconds to search.
        /// </summary>
        public int MaxSearchSec = 20;
    }
}