using System;

namespace CreateAR.SpirePlayer.Test
{
    public static class RuntimeAssert
    {
        public static void AreEqual(object a, object b, string message)
        {
            if (!a.Equals(b))
            {
                throw new Exception(string.Format(
                    "[{0}] != [{1}] : {2}",
                    a, b, message));
            }
        }

        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(message);
            }
        }
    }
}