using System;
using System.Collections;
using System.Linq;
using System.Threading;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using ZXing;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public interface IQrReaderService
    {
        event Action<string> OnRead;

        IAsyncToken<Void> Start();
        IAsyncToken<Void> Stop();
    }

    public class QrDecoderThread
    {
        private readonly IBarcodeReader _reader = new BarcodeReader();

        private readonly Color32[] _colors;
        private readonly int _width;
        private readonly int _height;

        public event Action<string> OnSuccess;
        public event Action OnFail;

        public QrDecoderThread(Color32[] colors, int width, int height)
        {
            _colors = colors;
            _width = width;
            _height = height;
        }

        public void Start()
        {
            Log.Info(this, "Starting decode...");

            var result = _reader.Decode(_colors, _width, _height);
            if (null != result)
            {
                if (null != OnSuccess)
                {
                    OnSuccess(result.Text);
                }
            }
            else if (null != OnFail)
            {
                OnFail();
            }
        }
    }

    public class WsaQrReaderService : IQrReaderService
    {
        private readonly IBootstrapper _bootstrapper;

        private bool _isAlive;
        private bool _isReady;
        private AsyncToken<Void> _start;
        private AsyncToken<Void> _stop;

        private DateTime _lastPoll = DateTime.MinValue;

        private PhotoCapture _captureObject;

        public event Action<string> OnRead;

        public WsaQrReaderService(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        public IAsyncToken<Void> Start()
        {
            _start = new AsyncToken<Void>();

            _bootstrapper.BootstrapCoroutine(StartPoll());

            PhotoCapture.CreateAsync(false, OnCapture);

            return _start;
        }

        public IAsyncToken<Void> Stop()
        {
            _stop = new AsyncToken<Void>();

            _isAlive = false;

            if (null != _captureObject)
            {
                _captureObject.StopPhotoModeAsync(OnPhotoModeStopped);
            }
            else
            {
                _stop.Succeed(Void.Instance);
            }

            return _stop;
        }

        private IEnumerator StartPoll()
        {
            _isAlive = true;

            while (_isAlive)
            {
                if (_isReady && DateTime.Now.Subtract(_lastPoll).TotalSeconds > 0.5f)
                {
                    Poll();
                }

                yield return null;
            }
        }

        private void Poll()
        {
            _lastPoll = DateTime.Now;
            _captureObject.TakePhotoAsync(OnCapturedToMemory);
        }

        private void OnCapturedToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {
            if (result.success)
            {
                // Create our Texture2D for use and set the correct resolution
                var cameraResolution = PhotoCapture
                    .SupportedResolutions
                    .OrderByDescending(res => res.width * res.height)
                    .First();
                var texture = new Texture2D(cameraResolution.width, cameraResolution.height);
                
                // Copy the raw image data into our target texture
                photoCaptureFrame.UploadImageDataToTexture(texture);
                
                // scan
                var decoder = new QrDecoderThread(texture.GetPixels32(), texture.width, texture.height);
                decoder.OnSuccess += Decoder_OnDecoded;
                decoder.OnFail += Decoder_OnFail;
                ThreadPool.QueueUserWorkItem(_ => decoder.Start());
            }
        }

        private void Decoder_OnDecoded(string value)
        {
            if (null != OnRead)
            {
                OnRead(value);
            }
        }

        private void Decoder_OnFail()
        {

        }

        private void OnCapture(PhotoCapture captureObject)
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

            _captureObject = captureObject;
            _captureObject.StartPhotoModeAsync(parameters, OnPhotoModeStarted);
        }

        private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {
                _isReady = true;
                _start.Succeed(Void.Instance);
            }
            else
            {
                _start.Fail(new Exception(string.Format(
                    "Could not start photo capture : {0}.",
                    result.resultType)));
            }
        }

        private void OnPhotoModeStopped(PhotoCapture.PhotoCaptureResult result)
        {
            _captureObject.Dispose();

            _stop.Succeed(Void.Instance);
        }
    }
}