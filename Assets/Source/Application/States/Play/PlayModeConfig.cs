using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Configuration for play mode.
    /// </summary>
    public class PlayModeConfig : MonoBehaviour
    {
        /// <summary>
        /// Events.
        /// </summary>
        public IUXEventHandler Events;

        /// <summary>
        /// Prefab for an anchor.
        /// </summary>
        public GameObject AnchorPrefab;
    }
}