using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.Dynamics
{
    [Serializable]
    public struct Target
    {
        /// <summary>
        /// Continuous Updates
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// Current Target Position
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Returns true if the target is empty
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
        /// Transform or position
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
