﻿using Enklu.Data;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Methods for vectors.
    /// </summary>
    public class Vec3Methods
    {
        /// <summary>
        /// Static instance.
        /// </summary>
        public static readonly Vec3Methods Instance = new Vec3Methods();
        
        /// <summary>
        /// Constants.
        /// </summary>
        public readonly Vec3 one = new Vec3(1, 1, 1);
        public readonly Vec3 zero = new Vec3(0, 0, 0);
        public readonly Vec3 up = new Vec3(0, 1, 0);
        public readonly Vec3 forward = new Vec3(0, 0, 1);
        public readonly Vec3 right = new Vec3(1, 0, 0);

        /// <summary>
        /// Constructor method.
        /// </summary>
        public static Vec3 create(float x, float y, float z)
        {
            return new Vec3(x, y, z);
        }

        /// <summary>
        /// Multiplies a vector by scalar.
        /// </summary>
        public Vec3 scale(Vec3 vec, float scalar)
        {
            return scalar * vec;
        }
        
        /// <summary>
        /// Adds two vectors.
        /// </summary>
        public Vec3 add(Vec3 a, Vec3 b)
        {
            return a + b;
        }

        /// <summary>
        /// Subracts two vectors.
        /// </summary>
        public Vec3 sub(Vec3 a, Vec3 b)
        {
            return a - b;
        }
        
        /// <summary>
        /// Dots two vectors.
        /// </summary>
        public float dot(Vec3 a, Vec3 b)
        {
            return Vec3.Dot(a, b);
        }

        /// <summary>
        /// Cross product of two vectors.
        /// </summary>
        public Vec3 cross(Vec3 a, Vec3 b)
        {
            return Vec3.Cross(a, b);
        }

        /// <summary>
        /// Angle between two vectors in degrees.
        /// </summary>
        public float angle(Vec3 a, Vec3 b)
        {
            return Vec3.Angle(a, b);
        }

        /// <summary>
        /// Vector length.
        /// </summary>
        public float len(Vec3 a)
        {
            return a.Magnitude;
        }

        /// <summary>
        /// Vector length squared.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public float lenSqr(Vec3 a)
        {
            return a.MagnitudeSqr;
        }

        /// <summary>
        /// Normalizes a vector.
        /// </summary>
        /// <param name="a">The vector to normalize.</param>
        /// <returns></returns>
        public Vec3 normalize(Vec3 a)
        {
            var mag = a.Magnitude;
            
            return new Vec3(
                a.x / mag,
                a.y / mag,
                a.z / mag);
        }

        /// <summary>
        /// Calculates the distance between two Vec3's. If possible, use distanceSqr for performance instead.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public float distance(Vec3 a, Vec3 b)
        {
            return Vec3.Distance(a, b);
        }

        /// <summary>
        /// Calculates the squared distance between two Vec3's.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public float distanceSqr(Vec3 a, Vec3 b)
        {
            return Vec3.DistanceSqr(a, b);
        }

        /// <summary>
        /// Calculates the horizontal distance between two Vec3's.
        /// If possible, use distanceXZSqr for performance instead.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public float distanceXZ(Vec3 a, Vec3 b)
        {
            return Vec3.DistanceXZ(a, b);
        }

        /// <summary>
        /// Calculates the squared horizontal distance between two Vec3's.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public float distanceXZSqr(Vec3 a, Vec3 b)
        {
            return Vec3.DistanceXZSqr(a, b);
        }
    }
}