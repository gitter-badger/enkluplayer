﻿using System;
using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Simple Vector class.
    /// </summary>
    [Serializable]
    public struct Vec3
    {
        /// <summary>
        /// Radians -> Degrees.
        /// </summary>
        private const float RAD2_DEG = (float) (360 / (2 * Math.PI));

        /// <summary>
        /// Default vector.
        /// </summary>
        public static readonly Vec3 Zero = new Vec3();

        /// <summary>
        /// Identity vector.
        /// </summary>
        public static readonly Vec3 One = new Vec3(1, 1, 1);

        /// <summary>
        /// Right vector.
        /// </summary>
        public static readonly Vec3 Right = new Vec3(1, 0, 0);

        /// <summary>
        /// Up vector.
        /// </summary>
        public static readonly Vec3 Up = new Vec3(0, 1, 0);

        /// <summary>
        /// Forward vector.
        /// </summary>
        public static readonly Vec3 Forward = new Vec3(0, 0, 1);

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
        /// Sets the components of this Vec3 from values.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        /// <returns></returns>
        public Vec3 Set(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            return this;
        }

        /// <summary>
        /// Vector string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("({0:0.00}, {1:0.00}, {2:0.00})", x, y, z);
        }

        /// <summary>
        /// Scalar magnitude of the vector.
        /// </summary>
        [JsonIgnore]
        public float Magnitude
        {
            get { return (float) Math.Sqrt(x * x + y * y + z * z); }
        }

        /// <summary>
        /// Scalar squared magnitude of the vector.
        /// </summary>
        [JsonIgnore]
        public float MagnitudeSqr
        {
            get { return x * x + y * y + z * z; }
        }

        /// <summary>
        /// Returns the same vector with magnitude of 1.
        /// </summary>
        [JsonIgnore]
        public Vec3 Normalized
        {
            get
            {
                var magnitude = Magnitude;
                if (magnitude < float.Epsilon)
                {
                    return Zero;
                }

                var magnitudeReciprical = 1.0f / magnitude;

                return new Vec3(
                    x * magnitudeReciprical, 
                    y * magnitudeReciprical, 
                    z * magnitudeReciprical);
            }
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
        /// Returns the dot product for two vectors.
        /// </summary>
        /// <param name="lhs">Left hand side of the cross product.</param>
        /// <param name="rhs">Right hand side of the cross product.</param>
        /// <returns></returns>
        public static Vec3 Cross(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(
                lhs.y*rhs.z - lhs.z*rhs.y,
                lhs.z*rhs.x - lhs.x*rhs.z,
                lhs.x*rhs.y - lhs.y*rhs.x);
        }

        /// <summary>
        /// Returns the angle between two vectors in degrees.
        /// </summary>
        /// <param name="lhs">Left hand side of the angle.</param>
        /// <param name="rhs">Right hand side of the angle.</param>
        /// <returns></returns>
        public static float Angle(Vec3 lhs, Vec3 rhs)
        {
            return (float) Math.Acos(Dot(lhs, rhs) / (lhs.Magnitude * rhs.Magnitude)) * RAD2_DEG;
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
        /// Returns true when component-wise approximately equal.
        /// </summary>
        /// <param name="lhs">Another vec3.</param>
        /// <param name="tolerance">Optional value to use for comparison.</param>
        /// <returns></returns>
        public bool Approximately(Vec3 lhs, float tolerance = float.Epsilon)
        {
            return Math.Abs(x - lhs.x) < tolerance
                && Math.Abs(y - lhs.y) < tolerance
                && Math.Abs(z - lhs.z) < tolerance;
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

        /// <summary>
        /// Tests strict equality of Vector components.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Vec3 a, Vec3 b)
        {
            return a.x == b.x
                && a.y == b.y
                && a.z == b.z;
        }

        /// <summary>
        /// Tests strict inequality of Vector components.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Vec3 a, Vec3 b)
        {
            return a.x != b.x
                || a.y != b.y
                || a.z != b.z;
        }

        /// <summary>
        /// Returns the squared distance between two Vec3's.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float DistanceSqr(Vec3 from, Vec3 to)
        {
            return (from - to).MagnitudeSqr;
        }

        /// <summary>
        /// Returns the distance between two Vec3's. If possible, use DistanceSqr for performance instead.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float Distance(Vec3 from, Vec3 to)
        {
            return (from - to).Magnitude;
        }

        /// <summary>
        /// Returns the squared horizontal distance between two Vec3's.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float DistanceXZSqr(Vec3 from, Vec3 to)
        {
            var diff = from - to;
            return diff.x * diff.x + diff.z * diff.z;
        }

        /// <summary>
        /// Returns the horizonstal distance between two Vec3's. If possible, use DistanceXZSqr for performance instead.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static float DistanceXZ(Vec3 from, Vec3 to)
        {
            return (float) Math.Sqrt(DistanceXZSqr(from, to));
        }
    }
}