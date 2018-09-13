using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Configuration for play mode.
    /// </summary>
    public class PlayModeConfig : MonoBehaviour
    {
        /// <summary>
        /// Prefab for an anchor.
        /// </summary>
        public AnchorRenderer AnchorPrefab;

        /// <summary>
        /// Prefab for a primary anchor.
        /// </summary>
        public AnchorRenderer PrimaryAnchorPrefab;

        /// <summary>
        /// Prefab for a container.
        /// </summary>
        public ContainerRenderer ContainerPrefab;
        
        /// <summary>
        /// Prefab for runtime gizmos.
        /// </summary>
        public GameObject RuntimeGizmoSystem;

        /// <summary>
        /// Control bar.
        /// </summary>
        public DesktopControlBarView ControlBar;
    }
}