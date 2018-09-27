namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public static class TestData
    {
        // A handful of rotations from different axes/quadrants
        public static Vec3[] EulerArray =
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
    }
}