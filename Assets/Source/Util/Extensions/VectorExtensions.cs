using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Extensions for vectors.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Creates a Unity vector from a Vec2.
        /// </summary>
        /// <param name="this">The vec2.</param>
        /// <returns></returns>
        public static Vector2 ToVector(this Vec2 @this)
        {
            return new Vector2(@this.x, @this.y);
        }

        /// <summary>
        /// Creates a Vec2 from a Unity Vector.
        /// </summary>
        /// <param name="this">The vector.</param>
        /// <returns></returns>
        public static Vec2 ToVec(this Vector2 @this)
        {
            return new Vec2(@this.x, @this.y);
        }

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

        /// <summary>
        /// Converts to a 2D vector ignoring the y component
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector2 ToXZ(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        /// <summary>
        /// Converts to a 2D vector ignoring the y component
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 ToX0Y(this Vector2 vector)
        {
            return new Vector3(vector.x, 0, vector.y);
        }

        /// <summary>
        /// Cross Product
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static float Cross(this Vector2 lhs, Vector2 rhs)
        {
            return lhs.x * rhs.y - lhs.y * rhs.x;
        }

        /// <summary>
        /// Cross Product
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static float Dot(this Vector2 lhs, Vector2 rhs)
        {
            return rhs.x * lhs.x + rhs.y * lhs.y;
        }

        /// <summary>
        /// Perpendicular Vector
        /// </summary>
        /// <param name="@this"></param>
        /// <returns></returns>
        public static Vector2 Perpendicular(this Vector2 @this)
        {
            return new Vector2(@this.y, -(@this.x));
        }
    }
}
