using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Connection that does nothing.
    /// </summary>
    public class PassthroughConnection : IConnection
    {
        /// <inheritdoc />
        public bool IsConnected { get; private set; }

        /// <inheritdoc />
        public event Action OnConnected;

        /// <inheritdoc />
        public IAsyncToken<Void> Connect(EnvironmentData environment)
        {
            return new AsyncToken<Void>(Void.Instance);
        }
    }
}