using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.Test
{
    public class AssertJsApi
    {
        public static readonly AssertJsApi Instance = new AssertJsApi();
         
        public void areEqual(object a, object b, string message)
        {
            RuntimeAssert.AreEqual(a, b, message);
        }

        public void isTrue(bool condition, string message)
        {
            RuntimeAssert.IsTrue(condition, message);
        }

        public void isNull(object obj, string message)
        {
            RuntimeAssert.IsNull(obj, message);
        }
    }
    
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
                throw new Exception(string.Format("Expected true but got false : {0}.", message));
            }
        }

        public static void IsNull(object obj, string message)
        {
            if (null != obj)
            {
                throw new Exception(string.Format("Expected null but got {0} : {1}.", obj, message));
            }
        }
    }
}