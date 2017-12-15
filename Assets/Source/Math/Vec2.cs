using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Simple Vector class.
    /// </summary>
    [Serializable]
    public struct Vec2
    {
        /// <summary>
        /// Default vector.
        /// </summary>
        public static readonly Vec2 Zero = new Vec2();

        /// <summary>
        /// Identity vector.
        /// </summary>
        public static readonly Vec2 One = new Vec2(1, 1);

        /// <summary>
        /// X component.
        /// </summary>
        public float x;

        /// <summary>
        /// Y component.
        /// </summary>
        public float y;
        
        /// <summary>
        /// Creates a Vec2 from a Vec2.
        /// </summary>
        /// <param name="vector">Source vector.</param>
        public Vec2(Vec2 vector)
        {
            x = vector.x;
            y = vector.y;
        }

        /// <summary>
        /// Creates a Vec2 from components.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Vector string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0:0.00}, {1:0.00}", x, y);
        }

        /// <summary>
        /// Scalar magnitude of the vector.
        /// </summary>
        public float Magnitude
        {
            get { return (float) Math.Sqrt(x * x + y * y); }
        }

        /// <summary>
        /// Returns the same vector with magnitude of 1.
        /// </summary>
        public Vec2 Normalized
        {
            get
            {
                var magnitude = Magnitude;
                if (magnitude < float.Epsilon)
                {
                    return Zero;
                }

                var magnitudeReciprical = 1.0f / magnitude;

                return new Vec2(
                    x * magnitudeReciprical,
                    y * magnitudeReciprical);
            }
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="vector">Multiplicand.</param>
        /// <param name="multiplier">Scalar multiplier.</param>
        /// <returns></returns>
        public static Vec2 operator *(Vec2 vector, float multiplier)
        {
            return new Vec2(
                vector.x * multiplier,
                vector.y * multiplier);
        }

        /// <summary>
        /// Multiplies a vector by a scalar.
        /// </summary>
        /// <param name="multiplier">Scalar multiplier.</param>
        /// <param name="vector">Multiplicand.</param>
        /// <returns></returns>
        public static Vec2 operator *(float multiplier, Vec2 vector)
        {
            return new Vec2(
                vector.x * multiplier,
                vector.y * multiplier);
        }

        /// <summary>
        /// Returns the dot product for two vectors.
        /// </summary>
        /// <param name="lhs">Left hand side of the dot product.</param>
        /// <param name="rhs">Right hand side of the dot product.</param>
        /// <returns></returns>
        public static float Dot(Vec2 lhs, Vec2 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        /// <summary>
        /// Interpolates from one vector to another.
        /// </summary>
        /// <param name="from">Source vector</param>
        /// <param name="to">Target vector</param>
        /// <param name="t">Factor of interpolation [0..1]</param>
        /// <returns></returns>
        public static Vec2 Lerp(Vec2 from, Vec2 to, float t)
        {
            return new Vec2(
                from.x + (to.x - from.x) * t,
                from.y + (to.y - from.y) * t);
        }

        /// <summary>
        /// Returns true when component-wise approximately equal.
        /// </summary>
        /// <param name="lhs">Another Vec2.</param>
        /// <returns></returns>
        public bool Approximately(Vec2 lhs)
        {
            return Math.Abs(x - lhs.x) < float.Epsilon
                   && Math.Abs(y - lhs.y) < float.Epsilon;
        }

        /// <summary>
        /// Component-wise addition.
        /// </summary>
        /// <param name="lhs">Right hand side.</param>
        /// <param name="rhs">Left hand side.</param>
        /// <returns></returns>
        public static Vec2 operator +(Vec2 lhs, Vec2 rhs)
        {
            return new Vec2(
                lhs.x + rhs.x,
                lhs.y + rhs.y);
        }

        /// <summary>
        /// Component-wise subtraction.
        /// </summary>
        /// <param name="lhs">Right hand side.</param>
        /// <param name="rhs">Left hand side.</param>
        /// <returns></returns>
        public static Vec2 operator -(Vec2 lhs, Vec2 rhs)
        {
            return new Vec2(
                lhs.x - rhs.x,
                lhs.y - rhs.y);
        }
    }
}