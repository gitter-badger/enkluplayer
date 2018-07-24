﻿using UnityEngine;

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
        public AnchorRenderer AnchorPrefab;

        /// <summary>
        /// Prefab for a container.
        /// </summary>
        public ContainerRenderer ContainerPrefab;
        
        /// <summary>
        /// Prefab for runtime gizmos.
        /// </summary>
        public GameObject RuntimeGizmoSystem;
    }
}