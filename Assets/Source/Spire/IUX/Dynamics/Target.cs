using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX.Dynamics
{
    /// <summary>
    /// Describes a set of target criteria for a magnet.
    /// </summary>
    [Serializable]
    public struct Target
    {
        /// <summary>
        /// The transform to track.
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// Current position of the target..
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Returns true if the target is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Transform == null
                    && Position.Approximately(Vector3.zero);
            }
        }

        /// <summary>
        /// Calculates the position.
        /// </summary>
        public Vector3 DynamicPosition
        {
            get
            {
                if (Transform != null)
                {
                    Position = Transform.position;
                }

                return Position;
            }
        }
    }
}
