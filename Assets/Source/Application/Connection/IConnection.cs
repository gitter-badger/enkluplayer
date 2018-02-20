using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a connection to an endpoint.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Connect.
        /// </summary>
        /// <param name="environment">The environment to connect to.</param>
        /// <returns></returns>
        IAsyncToken<Void> Connect(EnvironmentData environment);
    }
}