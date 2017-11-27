using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Extensions for Color.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Creates a Col4 from a Unity Color.
        /// </summary>
        /// <param name="this">The Color.</param>
        /// <returns></returns>
        public static Col4 ToCol(this Color @this)
        {
            return new Col4(@this.r, @this.g, @this.b, @this.a);
        }

        /// <summary>
        /// Col4 to Color.
        /// </summary>
        /// <param name="this">The Col4.</param>
        /// <returns></returns>
        public static Color ToColor(this Col4 @this)
        {
            return new Color(@this.r, @this.g, @this.b, @this.a);
        }
    }
}
