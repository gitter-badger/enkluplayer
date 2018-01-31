using System;

namespace CreateAR.SpirePlayer
{
    [Serializable]
    public class PropData
    {
        public string ContentId;

        public Vec3 Position;
        public Vec3 Rotation;
        public Vec3 Scale = Vec3.One;
        public Vec3 CenterOffset;
        public Vec3 Size;

        public bool Fade = true;
    }
}