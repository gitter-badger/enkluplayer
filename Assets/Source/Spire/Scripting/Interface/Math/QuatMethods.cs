using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class QuatMethods
    {
        public static readonly QuatMethods Instance = new QuatMethods();
        
        public readonly Quat identity = Quaternion.identity.ToQuat();

        public static Quat create(float x, float y, float z)
        {
            return Quat.Euler(x, y, z);
        }
        
        public Quat eul(float x, float y, float z)
        {
            return Quaternion.Euler(x, y, z).ToQuat();
        }
    }
}