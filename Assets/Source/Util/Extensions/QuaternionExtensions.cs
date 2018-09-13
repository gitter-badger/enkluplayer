using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Extensions for quaternions.
    /// </summary>
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Creates a Quat.
        /// </summary>
        /// <param name="this">The Quaternion.</param>
        /// <returns></returns>
        public static Quat ToQuat(this Quaternion @this)
        {
            return new Quat(@this.x, @this.y, @this.z, @this.w);
        }

        /// <summary>
        /// Creates a Quaternion.
        /// </summary>
        /// <param name="this">The quat.</param>
        /// <returns></returns>
        public static Quaternion ToQuaternion(this Quat @this)
        {
            return new Quaternion(@this.x, @this.y, @this.z, @this.w);
        }
        
        /// <summary>
        /// True iff the two quaternions are component-wise withing epsilon.
        /// </summary>
        /// <param name="this">The quaternion.</param>
        /// <param name="rhs">A quaternion to test against.</param>
        /// <returns></returns>
        public static bool Approximately(this Quaternion @this, Quaternion rhs)
        {
            return Mathf.Approximately(@this.x, rhs.x)
                   && Mathf.Approximately(@this.y, rhs.y)
                   && Mathf.Approximately(@this.z, rhs.z)
                   && Mathf.Approximately(@this.w, rhs.w);
        }
    }
}