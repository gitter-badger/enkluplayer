using System;
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
        /// True iff connection is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Called when <c>IsConnected</c> changes from false to true.
        /// </summary>
        event Action OnConnected;

        /// <summary>
        /// Connect.
        /// </summary>
        /// <param name="environment">The environment to connect to.</param>
        /// <returns></returns>
        IAsyncToken<Void> Connect(EnvironmentData environment);
    }
}