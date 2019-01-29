using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Passthrough implementation of IImageCapture
    /// </summary>
    public class PassthroughImageCapture : IImageCapture
    {
        /// <inheritdoc />
        public Action<string> OnImageCreated { get; set; }

        /// <inheritdoc />
        public IAsyncToken<Void> Warm()
        {
            return new AsyncToken<Void>(new NotSupportedException());
        }

        /// <inheritdoc />
        public IAsyncToken<string> Capture(string customPath = null)
        {
            return new AsyncToken<string>(new NotSupportedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Abort()
        {
            return new AsyncToken<Void>(new NotSupportedException());
        }
    }
}