using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Extensions for Rect and Rectangle.
    /// </summary>
    public static class RectangleExtensions
    {
        /// <summary>
        /// Converts to Rect.
        /// </summary>
        /// <param name="this">Rectangle.</param>
        /// <returns></returns>
        public static Rect ToRect(this Rectangle @this)
        {
            return new Rect(
                @this.min.x, @this.min.y,
                @this.size.x, @this.size.y);
        }

        /// <summary>
        /// Converts to Rectangle.
        /// </summary>
        /// <param name="this">Rect.</param>
        /// <returns></returns>
        public static Rectangle ToRectangle(this Rect @this)
        {
            return new Rectangle(
                @this.min.x, @this.min.y,
                @this.size.x, @this.size.y);
        }

        /// <summary>
        /// True iff rectangles are approximately equal.
        /// </summary>
        /// <param name="this">This rect.</param>
        /// <param name="other">Another rect to measure against.</param>
        /// <returns></returns>
        public static bool Approximately(this Rectangle @this, Rectangle other)
        {
            return Mathf.Approximately(@this.min.x, other.min.x)
                   && Mathf.Approximately(@this.max.x, other.max.x)
                   && Mathf.Approximately(@this.min.y, other.min.y)
                   && Mathf.Approximately(@this.max.y, other.max.y);
        }
    }
}