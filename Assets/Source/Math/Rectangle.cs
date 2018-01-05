using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Rectangle!
    /// </summary>
    [Serializable]
    public struct Rectangle
    {
        /// <summary>
        /// Min.
        /// </summary>
        public Vec2 min;

        /// <summary>
        /// Max.
        /// </summary>
        public Vec2 max;

        /// <summary>
        /// Size.
        /// </summary>
        public Vec2 size
        {
            get
            {
                return max - min;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Rectangle(float x, float y, float width, float height)
        {
            min = new Vec2(x, y);
            max = new Vec2(x + width, y + height);
        }
    }
}