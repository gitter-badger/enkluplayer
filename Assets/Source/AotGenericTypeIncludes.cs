using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Type includes that killed by AoT. Link.xml doesn't seem to be able to
    /// handle generic types.
    /// </summary>
    public static class AotGenericTypeIncludes
    {
        /// <summary>
        /// Include stuff.
        /// </summary>
        public static void Include()
        {
            new List<float>();
            new List<int>();
        }
    }
}