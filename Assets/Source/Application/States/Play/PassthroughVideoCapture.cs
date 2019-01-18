using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Passthrough implementation of IVideoCapture
    /// </summary>
    public class PassthroughVideoCapture : IVideoCapture
    {
        /// <inheritdoc />
        public bool IsRecording
        {
            get { return false; }
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Warm()
        {
            return new AsyncToken<Void>(new NotSupportedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start()
        {
            return new AsyncToken<Void>(new NotSupportedException());
        }

        /// <inheritdoc />
        public IAsyncToken<string> Stop()
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