using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Stores data for a Vector tween.
    /// </summary>
    [Serializable]
    public class VectorTweenData
    {
        /// <summary>
        /// Start of tween.
        /// </summary>
        public Vector3 Start;

        /// <summary>
        /// End of tween.
        /// </summary>
        public Vector3 End;

        /// <summary>
        /// Curve to interpolate with.
        /// </summary>
        public AnimationCurve Curve;
    }
}