using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Configuration for play mode.
    /// </summary>
    public class PlayModeConfig : MonoBehaviour
    {
        /// <summary>
        /// Prefab for an anchor.
        /// </summary>
        public GameObject AnchorPrefab;

        /// <summary>
        /// Prefab for loading.
        /// </summary>
        public GameObject LoadingPrefab;

        /// <summary>
        /// Prefab for runtime gizmos.
        /// </summary>
        public GameObject RuntimeGizmoSystem;
    }
}