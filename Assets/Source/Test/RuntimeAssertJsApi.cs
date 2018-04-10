namespace CreateAR.SpirePlayer.Test
{
    /// <summary>
    /// Js api for RuntimeAssert.
    /// </summary>
    public class RuntimeAssertJsApi
    {
        /// <summary>
        /// Static, reusable instance.
        /// </summary>
        public static readonly RuntimeAssertJsApi Instance = new RuntimeAssertJsApi();
         
        /// <summary>
        /// Asserts that two objects are equal.
        /// </summary>
        public void areEqual(object a, object b, string message)
        {
            RuntimeAssert.AreEqual(a, b, message);
        }

        /// <summary>
        /// Asserts that a condition holds true.
        /// </summary>
        public void isTrue(bool condition, string message)
        {
            RuntimeAssert.IsTrue(condition, message);
        }

        /// <summary>
        /// Asserts that an object is null.
        /// </summary>
        public void isNull(object obj, string message)
        {
            RuntimeAssert.IsNull(obj, message);
        }
    }
}