using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Creates a Vec3 from a Unity Vector.
        /// </summary>
        /// <param name="this">The vector.</param>
        /// <returns></returns>
        public static Col4 ToCol(this Color @this)
        {
            return new Col4(@this.r, @this.g, @this.b, @this.a);
        }
    }
}
