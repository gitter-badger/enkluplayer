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
        /// True iff edit mode is enabled.
        /// </summary>
        public bool EditModeEnabled = true;
    }
}