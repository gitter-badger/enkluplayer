using System;
using Enklu.Data;
using UnityEngine;

namespace CreateAR.EnkluPlayer
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

        /// <summary>
        /// True iff colors are approximately equal.
        /// </summary>
        /// <param name="this">This color.</param>
        /// <param name="color">Comparison color.</param>
        /// <returns></returns>
        public static bool Approximately(this Col4 @this, Col4 color)
        {
            return Math.Abs(@this.r - color.r) < float.Epsilon
                && Math.Abs(@this.g - color.g) < float.Epsilon
                && Math.Abs(@this.b - color.b) < float.Epsilon
                && Math.Abs(@this.a - color.a) < float.Epsilon;
        }
    }
}
