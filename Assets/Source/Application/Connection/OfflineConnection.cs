using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Offline connections.
    /// </summary>
    public class OfflineConnection : IConnection
    {
        /// <inheritdoc />
        public bool IsConnected { get; private set; }

        /// <inheritdoc />
        public IAsyncToken<Void> Connect(EnvironmentData environment)
        {
            return new AsyncToken<Void>(new Exception("Offline."));
        }
    }
}