using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Offline connections.
    /// </summary>
    public class OfflineConnection : IConnection
    {
        /// <inheritdoc />
        public bool IsConnected { get; private set; }

        /// <inheritdoc />
        public event Action OnConnected;

        /// <inheritdoc />
        public IAsyncToken<Void> Connect(EnvironmentData environment)
        {
            return new AsyncToken<Void>(new Exception("Offline."));
        }

        /// <summary>
        /// Forces even to be called.
        /// </summary>
        public void ForceConnect()
        {
            if (null != OnConnected)
            {
                OnConnected();
            }
        }
    }
}