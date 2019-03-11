using Enklu.Data;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Methods for Col4.
    /// </summary>
    public class Col4Methods
    {
        /// <summary>
        /// Static instance.
        /// </summary>
        public static Col4Methods Instance = new Col4Methods();

        /// <summary>
        /// Creates a col4.
        /// </summary>
        /// <param name="r">Red.</param>
        /// <param name="g">Green.</param>
        /// <param name="b">Blue.</param>
        /// <param name="a">Alpha.</param>
        /// <returns></returns>
        public static Col4 create(float r, float g, float b, float a)
        {
            return new Col4(r, g, b, a);
        }

        /// <summary>
        /// Linearly interpolate between two colors.
        /// </summary>
        public Col4 lerp(Col4 from, Col4 to, float t)
        {
            return Col4.Lerp(from, to, t);
        }

        /// <summary>
        /// Multiplies two colors.
        /// </summary>
        public Col4 mul(Col4 a, Col4 b)
        {
            return new Col4(
                a.r * b.r,
                a.g * b.g,
                a.b * b.b,
                a.a * b.a);
        }
    }
}