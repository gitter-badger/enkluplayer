using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Describes a long running service that scans QR codes through the camera.
    /// </summary>
    public interface IQrReaderService
    {
        /// <summary>
        /// Called when the service has read a value.
        /// </summary>
        event Action<string> OnRead;

        /// <summary>
        /// Starts up the service.
        /// </summary>
        IAsyncToken<Void> Start();

        /// <summary>
        /// Stops the service.
        /// </summary>
        IAsyncToken<Void> Stop();
    }

    public class UnsupportedQrReaderService : IQrReaderService
    {
        public event Action<string> OnRead;

        public IAsyncToken<Void> Start()
        {
            throw new NotImplementedException();
        }

        public IAsyncToken<Void> Stop()
        {
            throw new NotImplementedException();
        }
    }
}