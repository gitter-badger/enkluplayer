using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Strategy for login.
    /// </summary>
    public interface ILoginStrategy
    {
        /// <summary>
        /// Logs in.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Login();
    }
}