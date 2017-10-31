namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Simple Vector class.
    /// </summary>
    public struct Vec3
    {
        /// <summary>
        /// X component.
        /// </summary>
        public float x;

        /// <summary>
        /// Y component.
        /// </summary>
        public float y;

        /// <summary>
        /// Z component.
        /// </summary>
        public float z;

        /// <summary>
        /// Creates a Vec3 from a Vec3.
        /// </summary>
        /// <param name="vector">Source vector.</param>
        public Vec3(Vec3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        /// <summary>
        /// Crates a Vec3 from components.
        /// </summary>
        /// <param name="x">X component.</param>
        /// <param name="y">Y component.</param>
        /// <param name="z">Z component.</param>
        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}