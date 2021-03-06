﻿using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Qr
{
    /// <summary>
    /// Implementation on unsupported platforms.
    /// </summary>
    public class UnsupportedQrReaderService : IQrReaderService
    {
        /// <inheritdoc />
        public event Action<string> OnRead;

        /// <inheritdoc />
        public IAsyncToken<Void> Start()
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Stop()
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }

        /// <summary>
        /// Forces OnRead to be called.
        /// </summary>
        public void ForceRead()
        {
            if (null != OnRead)
            {
                OnRead(string.Empty);
            }
        }
    }
}