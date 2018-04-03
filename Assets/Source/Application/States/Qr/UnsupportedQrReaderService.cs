using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
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
    }
}