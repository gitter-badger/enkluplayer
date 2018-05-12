#if NETFX_CORE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.Qr
{
    /// <summary>
    /// Service that reads in QR codes from webcam. This implementation is for Wsa.
    /// </summary>
    public class WsaQrReaderService : IQrReaderService
    {
        private readonly IBootstrapper _bootstrapper;
        private readonly List<Action> _queuedActions = new List<Action>();
        private readonly List<Action> _queuedActionsReadBuffer = new List<Action>();

        private bool _isAlive;
        private bool _isReady;
        private AsyncToken<Void> _start;
        private AsyncToken<Void> _stop;

        private DateTime _lastCapture = DateTime.MinValue;

        private PhotoCapture _captureObject;

        private QrDecoderWorker _worker;

        private Texture2D _texture;

        public event Action<string> OnRead;

        public WsaQrReaderService(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start()
        {
            _start = new AsyncToken<Void>();

            // start worker
            _worker = new QrDecoderWorker(_bootstrapper);
            _worker.OnSuccess += Decoder_OnDecoded;
            _worker.OnFail += Decoder_OnFail;

            Windows.System.Threading.ThreadPool.RunAsync(_ => _worker.Start());

            // start main thread poll
            _bootstrapper.BootstrapCoroutine(StartSynchronizeLoop());

            // start capture process
            PhotoCapture.CreateAsync(false, Thread_OnCaptureModeStarted);

            return _start;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Stop()
        {
            _stop = new AsyncToken<Void>();

            _isAlive = false;
            _worker.Stop();

            if (null != _captureObject)
            {
                _captureObject.StopPhotoModeAsync(Thread_OnPhotoModeStopped);
            }
            else
            {
                _stop.Succeed(Void.Instance);
            }

            return _stop;
        }

        /// <summary>
        /// Starts a coroutine that pulls messages to the main thread.
        /// </summary>
        private IEnumerator StartSynchronizeLoop()
        {
            _isAlive = true;

            while (_isAlive)
            {
                // pool if necessary
                if (_isReady && DateTime.Now.Subtract(_lastCapture).TotalSeconds > 0.5f)
                {
                    StartCapture();
                }

                // copy queued actions
                lock (_queuedActions)
                {
                    if (_queuedActions.Count > 0)
                    {
                        _queuedActionsReadBuffer.AddRange(_queuedActions);
                        _queuedActions.Clear();
                    }
                }

                // execute queued actions
                if (_queuedActionsReadBuffer.Count > 0)
                {
                    for (var i = 0; i < _queuedActionsReadBuffer.Count; i++)
                    {
                        _queuedActionsReadBuffer[i]();
                    }
                    _queuedActionsReadBuffer.Clear();
                }

                yield return null;
            }
        }

        /// <summary>
        /// Starts a capture.
        /// </summary>
        private void StartCapture()
        {
            _lastCapture = DateTime.Now;
            
            _captureObject.TakePhotoAsync(EndCapture);
        }

        /// <summary>
        /// Called by MS API when a capture is complete.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="photoCaptureFrame">Captured image.</param>
        private void EndCapture(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            // only worry about success
            if (result.success)
            {
                // This MUST happen on main thread.
                lock (_queuedActions)
                {
                    _queuedActions.Add(() =>
                    {
                        // Copy the raw image data into our target texture
                        photoCaptureFrame.UploadImageDataToTexture(_texture);
                        
                        // queue for read
                        _worker.Enqueue(_texture.GetPixels32(), _texture.width, _texture.height);
                    });
                }
            }
        }

        /// <summary>
        /// Called when the decoder has decoded something. This is guaranteed to
        /// be called on the main thread.
        /// </summary>
        /// <param name="id">Id of the capture.</param>
        /// <param name="value">Value.</param>
        private void Decoder_OnDecoded(int id, string value)
        {
            if (null != OnRead)
            {
                OnRead(value);
            }
        }

        /// <summary>
        /// Called when the decoder fails to decode something. This is guaranteed
        /// to be called on the main thread.
        /// </summary>
        /// <param name="id">Id of the capture.</param>
        private void Decoder_OnFail(int id)
        {

        }

        /// <summary>
        /// Called by the MS API in a threadpool when capture mode has been
        /// enabled.
        /// </summary>
        /// <param name="captureObject">The API.</param>
        private void Thread_OnCaptureModeStarted(PhotoCapture captureObject)
        {
            var cameraResolution = PhotoCapture
                .SupportedResolutions
                .OrderByDescending(res => res.width * res.height)
                .First();

            var parameters = new CameraParameters
            {
                hologramOpacity = 0.0f,
                cameraResolutionWidth = cameraResolution.width,
                cameraResolutionHeight = cameraResolution.height,
                pixelFormat = CapturePixelFormat.BGRA32
            };

            // create texture here
            _texture = new Texture2D(cameraResolution.width, cameraResolution.height);
            
            _captureObject = captureObject;
            _captureObject.StartPhotoModeAsync(parameters, Thread_OnPhotoModeStarted);
        }

        /// <summary>
        /// Called by the MS API in a threadpool when the photo mode has started.
        /// </summary>
        /// <param name="result">Result indicating success.</param>
        private void Thread_OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                _isReady = true;

                lock (_queuedActions)
                {
                    _queuedActions.Add(() => _start.Succeed(Void.Instance));
                }
            }
            else
            {
                lock (_queuedActions)
                {
                    _queuedActions.Add(() => _start.Fail(new Exception(string.Format(
                        "Could not start photo capture : {0}.",
                        result.resultType))));
                }
            }
        }

        /// <summary>
        /// Called by the MS API in a threadpool when photo mode has been shut
        /// off.
        /// </summary>
        /// <param name="result">True iff successful.</param>
        private void Thread_OnPhotoModeStopped(PhotoCapture.PhotoCaptureResult result)
        {
            _captureObject.Dispose();

            lock (_queuedActions)
            {
                _queuedActions.Add(() => _stop.Succeed(Void.Instance));
            }
        }
    }
}
#endif