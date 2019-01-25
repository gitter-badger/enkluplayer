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
        public Action<string> OnVideoCreated { get; set; }
        
        /// <inheritdoc />
        public bool IsRecording
        {
            get { return false; }
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Setup()
        {
            return new AsyncToken<Void>(new NotSupportedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start(string customPath = null)
        {
            return new AsyncToken<Void>(new NotSupportedException());
        }

        /// <inheritdoc />
        public IAsyncToken<string> Stop()
        {
            return new AsyncToken<string>(new NotSupportedException());
        }

        /// <inheritdoc />
        public void Teardown()
        {
        }
    }
}