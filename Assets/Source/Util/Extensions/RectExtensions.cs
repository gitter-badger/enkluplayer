using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Extensions for Rects.
    /// </summary>
    public static class RectExtensions
    {
        /// <summary>
        /// True iff rectangles are approximately equal.
        /// </summary>
        /// <param name="this">This rect.</param>
        /// <param name="other">Another rect to measure against.</param>
        /// <returns></returns>
        public static bool Approximately(this Rect @this, Rect other)
        {
            return Mathf.Approximately(@this.xMin, other.xMin)
                   && Mathf.Approximately(@this.xMax, other.xMax)
                   && Mathf.Approximately(@this.yMin, other.yMin)
                   && Mathf.Approximately(@this.xMax, other.yMax);
        }
    }
}