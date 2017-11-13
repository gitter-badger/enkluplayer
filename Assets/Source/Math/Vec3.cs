using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Simple Vector class.
    /// </summary>
    public struct Vec3
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
        /// Creates a Vec3 from a Vec3.
        /// </summary>
        /// <param name="vector">Source vector.</param>
        public Vec3(Vec3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        /// <summary>
        /// Crates a Vec3 from components.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Returns true when component-wise approximately equal.
        /// </summary>
        /// <param name="lhs">Another vec3.</param>
        /// <returns></returns>
        public bool Approximately(Vec3 lhs)
        {
            return Math.Abs(x - lhs.x) < float.Epsilon
                && Math.Abs(y - lhs.y) < float.Epsilon
                && Math.Abs(z - lhs.z) < float.Epsilon;
        }
    
        /// <summary>
        /// Component-wise addition.
        /// </summary>
        /// <param name="lhs">Right hand side.</param>
        /// <param name="rhs">Left hand side.</param>
        /// <returns></returns>
        public static Vec3 operator +(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(
                lhs.x + rhs.x,
                lhs.y + rhs.y,
                lhs.z + rhs.z);
        }
        
        /// <summary>
        /// Component-wise subtraction.
        /// </summary>
        /// <param name="lhs">Right hand side.</param>
        /// <param name="rhs">Left hand side.</param>
        /// <returns></returns>
        public static Vec3 operator -(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(
                lhs.x - rhs.x,
                lhs.y - rhs.y,
                lhs.z - rhs.z);
        }
    }
}