using UnityEngine;

namespace CreateAR.SpirePlayer
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
        public readonly Quat identity = Quaternion.identity.ToQuat();

        /// <summary>
        /// Creates a new Quat.
        /// </summary>
        public static Quat create(float x, float y, float z, float w)
        {
            return new Quat(x, y, z, w);
        }
        
        /// <summary>
        /// Creates a Quat from Euler rotations.
        /// </summary>
        public Quat euler(float x, float y, float z)
        {
            return Quaternion.Euler(x, y, z).ToQuat();
        }
    }
}