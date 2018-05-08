﻿using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Connection that does nothing.
    /// </summary>
    public class PassthroughConnection : IConnection
    {
        /// <inheritdoc />
        public bool IsConnected { get; private set; }

        /// <inheritdoc />
        public IAsyncToken<Void> Connect(EnvironmentData environment)
        {
            return new AsyncToken<Void>(Void.Instance);
        }
    }
}