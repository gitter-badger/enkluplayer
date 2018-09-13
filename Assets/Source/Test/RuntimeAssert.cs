using System;
using System.Collections.Generic;

namespace CreateAR.EnkluPlayer.Test
{
    /// <summary>
    /// Asserts for runtime.
    /// </summary>
    public static class RuntimeAssert
    {
        /// <summary>
        /// Asserts that two objects are equal.
        /// </summary>
        public static void AreEqual(object a, object b, string message)
        {
            if (!a.Equals(b))
            {
                throw new Exception(string.Format(
                    "[{0}] != [{1}] : {2}",
                    a, b, message));
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception(string.Format("Expected true but got false : {0}.", message));
            }
        }

        /// <summary>
        /// Asserts that an object is null.
        /// </summary>
        public static void IsNull(object obj, string message)
        {
            if (null != obj)
            {
                throw new Exception(string.Format("Expected null but got {0} : {1}.", obj, message));
            }
        }
    }
}