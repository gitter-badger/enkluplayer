using UnityEngine;

namespace CreateAR.Spire
{
    /// <summary>
    /// Extensions for vectors.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Checks two vectors for equality.
        /// </summary>
        public static bool Approximately(this Vector3 lhs, Vector3 rhs)
        {
            return lhs.Approximately(rhs, Mathf.Epsilon);
        }

        /// <summary>
        /// Check two vectors for equality using an epsilon.
        /// </summary>
        public static bool Approximately(this Vector3 lhs, Vector3 rhs, float epsilon)
        {
            var delta = lhs - rhs;
            var deltaMagSqr = delta.sqrMagnitude;
            return deltaMagSqr < epsilon;
        }
    }
}
