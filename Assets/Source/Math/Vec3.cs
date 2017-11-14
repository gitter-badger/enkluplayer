using UnityEngine;

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
        /// Creates a Vec3 from components.
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
        /// Vector string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0:0.00}, {1:0.00}, {2:0.00}", x, y, z);
        }

        /// <summary>
        /// Scalar magnitude of the vector.
        /// </summary>
        public float Magnitude
        {
            get { return Mathf.Sqrt(x * x + y * y + z * z); }
        }

        /// <summary>
        /// Returns the same vector with magnitude of 1.
        /// </summary>
        public Vec3 Normalized
        {
            get
            {
                var magnitude = Magnitude;
                if (magnitude < Mathf.Epsilon)
                {
                    return Vec3.Zero;
                }

                var magnitudeReciprical = 1.0f / magnitude;

                return new Vec3(
                    x * magnitudeReciprical, 
                    y * magnitudeReciprical, 
                    z * magnitudeReciprical);
            }
        }

        /// <summary>
        /// Adds a vector to another vector.
        /// </summary>
        /// <param name="lhs">Left hand side of the addition.</param>
        /// <param name="rhs">Right hand side of the addition.</param>
        /// <returns></returns>
        public static Vec3 operator+(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(
                lhs.x + rhs.x,
                lhs.y + rhs.y,
                lhs.z + rhs.z);
        }

        /// <summary>
        /// Subtracts a vector from another vector.
        /// </summary>
        /// <param name="lhs">Left hand side of the subtraction.</param>
        /// <param name="rhs">Right hand side of the subtraction.</param>
        /// <returns></returns>
        public static Vec3 operator -(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(
                lhs.x - rhs.x,
                lhs.y - rhs.y,
                lhs.z - rhs.z);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">Multiplicand.</param>
        /// <param name="multiplier">Scalar multiplier.</param>
        /// <returns></returns>
        public static Vec3 operator*(Vec3 vector, float multiplier)
        {
            return new Vec3(
                vector.x * multiplier,
                vector.y * multiplier,
                vector.z * multiplier);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="multiplier">Scalar multiplier.</param>
        /// <param name="vector">Multiplicand.</param>
        /// <returns></returns>
        public static Vec3 operator*(float multiplier, Vec3 vector)
        {
            return new Vec3(
                vector.x * multiplier,
                vector.y * multiplier,
                vector.z * multiplier);
        }

        /// <summary>
        /// Returns the dot product for two vectors.
        /// </summary>
        /// <param name="lhs">Left hand side of the dot product.</param>
        /// <param name="rhs">Right hand side of the dot product.</param>
        /// <returns></returns>
        public static float Dot(Vec3 lhs, Vec3 rhs)
        {
            return
                lhs.x * rhs.x
              + lhs.y * rhs.y
              + lhs.z * rhs.z;
        }

        /// <summary>
        /// Interpolates from one vector to another.
        /// </summary>
        /// <param name="from">Source vector</param>
        /// <param name="to">Target vector</param>
        /// <param name="t">Factor of interpolation [0..1]</param>
        /// <returns></returns>
        public static Vec3 Lerp(Vec3 from, Vec3 to, float t)
        {
            return new Vec3(
                from.x + (to.x - from.x) * t,
                from.y + (to.y - from.y) * t,
                from.z + (to.z - from.z) * t);
        }

        /// <summary>
        /// Default vector.
        /// </summary>
        public static readonly Vec3 Zero = new Vec3();

        /// <summary>
        /// Identity vector.
        /// </summary>
        public static readonly Vec3 One = new Vec3(1,1,1);
    }
}