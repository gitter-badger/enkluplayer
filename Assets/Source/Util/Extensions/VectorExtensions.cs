using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Extensions for vectors.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Creates a Unity vector from a Vec3.
        /// </summary>
        /// <param name="this">The vec3.</param>
        /// <returns></returns>
        public static Vector3 ToVector(this Vec3 @this)
        {
            return new Vector3(@this.x, @this.y, @this.z);
        }

        /// <summary>
        /// Creates a Vec3 from a Unity Vector.
        /// </summary>
        /// <param name="this">The vector.</param>
        /// <returns></returns>
        public static Vec3 ToVec(this Vector3 @this)
        {
            return new Vec3(@this.x, @this.y, @this.z);
        }

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
