using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// Anchor data structure.
    /// </summary>
    public class ArAnchor
    {
        /// <summary>
        /// Unique id of the anchor.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Center of the anchor in world space.
        /// </summary>
        public Vector3 Center { get; internal set; }

        /// <summary>
        /// Extents of the anchor in world space.
        /// </summary>
        public Vector3 Extents { get; internal set; }

        /// <summary>
        /// World space position.
        /// </summary>
        public Vector3 Position { get; internal set; }
        
        /// <summary>
        /// Worldspace rotation.
        /// </summary>
        public Quaternion Rotation { get; internal set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ArAnchor(string id)
        {
            Id = id;
        }
    }
}