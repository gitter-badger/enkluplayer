using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
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
        IAsyncToken<CredentialsData> Login();
    }
}