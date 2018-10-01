namespace CreateAR.EnkluPlayer.Test.Scripting
{
    /// <summary>
    /// A readonly set of data to be used across multiple tests.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// A handful of rotations from different axes/quadrants.
        /// </summary>
        public static readonly Vec3[] EulerArray =
        {
            new Vec3( 0,     0,     0),
            new Vec3( 90,    0,    0),
            new Vec3( 180,    0,    0),
            new Vec3( 270,    0,    0),

            new Vec3( 0,    0,    45),
            new Vec3( 0,    45,   0),
            new Vec3( 45,   0,    0),
            new Vec3( 0,    0,   -45),
            new Vec3( 0,   -45,   0),
            new Vec3(-45,   0,    0),
            new Vec3( 30,   60,   20),
            new Vec3( 30,   60,  -20),
            new Vec3( 30,  -60,   20),
            new Vec3( 30,  -60,  -20),
            new Vec3(-30,   60,   20),
            new Vec3(-30,   60,  -20),
            new Vec3(-30,  -60,   20),
            new Vec3(-30,  -60,  -20),
            new Vec3( 120,  160,  140),
            new Vec3( 120,  160, -140),
            new Vec3( 120, -160,  140),
            new Vec3( 120, -160, -140),
            new Vec3(-120,  160,  140),
            new Vec3(-120,  160, -140),
            new Vec3(-120, -160,  140),
            new Vec3(-120, -160, -140),
        };

        /// <summary>
        /// A handful of direction vectors from different axes/quadrants.
        /// </summary>
        public static readonly Vec3[] DirectionArray =
        {
            Vec3.Forward,
            Vec3.Right,
            Vec3.Up,
            new Vec3( 0,  0, -1),
            new Vec3( 0, -1,  0),
            new Vec3(-1,  0,  0),

            new Vec3( 1,  1,  1),
            new Vec3( 1,  1, -1),
            new Vec3( 1, -1,  1),
            new Vec3( 1, -1, -1),
            new Vec3(-1,  1,  1),
            new Vec3(-1,  1, -1),
            new Vec3(-1, -1,  1),
            new Vec3(-1, -1, -1)
        };
    }
}