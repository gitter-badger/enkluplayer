using NUnit.Framework.Interfaces;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Interface for objects that listen to <c>UnitTestRunner</c>.
    /// </summary>
    public interface IUnitTestListener
    {
        /// <summary>
        /// Called when a test has finished.
        /// </summary>
        /// <param name="result">The test result.</param>
        void OnFinishedTest(ITestResult result);

        /// <summary>
        /// Called when all tests have finished.
        /// </summary>
        void OnFinishedAll();
    }
}