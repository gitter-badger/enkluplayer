using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Quaternion wrapper for Js.
    /// </summary>
    public class QuatJs
    {
        /// <summary>
        /// X component.
        /// </summary>
        public float x;

        /// <summary>
        /// Y component.
        /// </summary>
        public float y;

        /// <summary>
        /// Z component.
        /// </summary>
        public float z;

        /// <summary>
        /// W component.
        /// </summary>
        public float w;

        /// <summary>
        /// Creates a Quaternion from components.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <param name="w">W component.</param>
        public QuatJs(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        /// <summary>
        /// Creates a Quaternion from a Unity quaternion.
        /// </summary>
        /// <param name="quaternion">The quaternion.</param>
        public QuatJs(Quaternion quaternion)
            : this(quaternion.x, quaternion.y, quaternion.z, quaternion.w)
        {
            //
        }

        /// <summary>
        /// Creates a Quaternion from Euler angles.
        /// </summary>
        /// <param name="euler">Euler angles.</param>
        public QuatJs(Vector3Js euler)
            : this(Quaternion.Euler(euler.x, euler.y, euler.z))
        {
            //
        }

        /// <summary>
        /// Creates a quaternion.
        /// </summary>
        /// <returns></returns>
        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }
}