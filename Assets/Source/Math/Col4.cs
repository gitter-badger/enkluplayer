namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Simple color class.
    /// </summary>
    public struct Col4
    {
        /// <summary>
        /// Identity color
        /// </summary>
        public static readonly Col4 White = new Col4(1f, 1f, 1f, 1f);

        /// <summary>
        /// X component.
        /// </summary>
        public float r;

        /// <summary>
        /// Y component.
        /// </summary>
        public float g;

        /// <summary>
        /// Z component.
        /// </summary>
        public float b;

        /// <summary>
        /// Z component.
        /// </summary>
        public float a;

        /// <summary>
        /// Creates a Color from a Color.
        /// </summary>
        /// <param name="color">Source color.</param>
        public Col4(Col4 color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        /// <summary>
        /// Col4 string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("col({0:0.00}, {1:0.00}, {2:0.00}, {3:0.00})", r, g, b, a);
        }

        /// <summary>
        /// Creates a Color from components.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <param name="a">Alpha component.</param>
        public Col4(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        
        /// <summary>
        /// Interpolates two colors
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Col4 Lerp(Col4 from, Col4 to, float t)
        {
            var color = new Col4(from);

            color.r += (to.r - from.r) * t;
            color.g += (to.g - from.g) * t;
            color.b += (to.b - from.b) * t;
            color.a += (to.a - from.a) * t;

            return color;
        }

        /// <summary>
        /// Multiplies one color by another color.
        /// </summary>
        /// <param name="lhs">Left hand side of the addition.</param>
        /// <param name="rhs">Right hand side of the addition.</param>
        /// <returns></returns>
        public static Col4 operator *(Col4 lhs, Col4 rhs)
        {
            return new Col4(
                lhs.r * rhs.r,
                lhs.g * rhs.g,
                lhs.b * rhs.b,
                lhs.a * rhs.a);
        }
    }
}
