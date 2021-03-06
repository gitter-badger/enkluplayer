﻿using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Qr
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
}