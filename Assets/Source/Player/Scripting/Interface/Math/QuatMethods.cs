﻿using Enklu.Data;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Quaternion methods.
    /// </summary>
    public class QuatMethods
    {
        /// <summary>
        /// Static instance that can be reused.
        /// </summary>
        public static readonly QuatMethods Instance = new QuatMethods();
        
        /// <summary>
        /// Identity rotation.
        /// </summary>
        public readonly Quat identity = Quat.Identity;

        /// <summary>
        /// Creates a new Quat.
        /// </summary>
        public static Quat create(float x, float y, float z, float w)
        {
            return new Quat(x, y, z, w);
        }

        /// <summary>
        /// Returns the inverse of a Quat.
        /// </summary>
        public static Quat inverse(Quat q)
        {
            return Quat.Inverse(q);
        }
        
        /// <summary>
        /// Creates a Quat from Euler rotations.
        /// </summary>
        public Quat euler(float x, float y, float z)
        {
            return Quat.Euler(x, y, z);
        }

        /// <summary>
        /// Creates a Quat from Euler rotations.
        /// </summary>
        public Quat eul(float x, float y, float z)
        {
            return Quat.Euler(x, y, z);
        }

        /// <summary>
        /// Creates a Quat that looks at the target Vec3 direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Quat fromToRotation(Vec3 forward, Vec3 direction)
        {
            return Quat.FromToRotation(forward, direction);
        }

        /// <summary>
        /// Returns a Vec3 representing a Vec3 forward rotated by a Quat.
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="forward"></param>
        /// <returns></returns>
        public Vec3 rotate(Quat rotation, Vec3 forward)
        {
            return Quat.Mult(rotation, forward);
        }
    }
}