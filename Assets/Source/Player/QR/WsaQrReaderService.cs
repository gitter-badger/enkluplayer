#if NETFX_CORE
using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using MediaFrameQrProcessing.Wrappers;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.Qr
{
    /// <summary>
    /// Service that reads in QR codes from webcam. This implementation is for Wsa.
    /// </summary>
    public class WsaQrReaderService : IQrReaderService
    {
        public event Action<string> OnRead;

        public WsaQrReaderService()
        {
            
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start()
        {
            Log.Info(this, "Scanning for QR codes.");

            ZXingQrCodeScanner.ScanFirstCameraForQrCode(result =>
                {
                    UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                    {
                        if (null != OnRead)
                        {
                            OnRead(result);
                        }
                    }, false);
                },
                null);

            return new AsyncToken<Void>(Void.Instance);
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Stop()
        {
            return new AsyncToken<Void>(Void.Instance);
        }
    }
}
#endif