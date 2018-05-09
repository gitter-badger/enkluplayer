#if UNITY_IOS
using System;
using System.Collections;
using System.Threading;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class IosQrReaderService : IQrReaderService
    {
        private const float CAPTURE_INTERVAL_SEC = 0.5f;

        private readonly IBootstrapper _bootstrapper;

        private bool _isAlive;
        private QrDecoderWorker _worker;
        private Color32[] _colors;
        private DateTime _lastCapture = DateTime.MinValue;
        private ScreenGrabber _grabber;

        public event Action<string> OnRead;

        public IosQrReaderService(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start()
        {
            var token = new AsyncToken<Void>();

            _grabber = Camera.main.gameObject.AddComponent<ScreenGrabber>();

            // start worker
            _worker = new QrDecoderWorker(_bootstrapper);
            _worker.OnSuccess += Decoder_OnDecoded;
            _worker.OnFail += Decoder_OnFail;
            
            ThreadPool.QueueUserWorkItem(_ => _worker.Start());

            // start capture on intreval
            _bootstrapper.BootstrapCoroutine(Capture());

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Stop()
        {
            Log.Info(this, "Stop iOS Qr Service.");
            
            var token = new AsyncToken<Void>();

            _isAlive = false;
            _worker.Stop();
            
            UnityEngine.Object.Destroy(_grabber);
            _grabber = null;

            return token;
        }

        /// <summary>
        /// Captures on an interval.
        /// </summary>
        private IEnumerator Capture()
        {
            _isAlive = true;

            while (_isAlive)
            {
                var now = DateTime.Now;
                if (now.Subtract(_lastCapture).TotalSeconds > CAPTURE_INTERVAL_SEC)
                {
                    _lastCapture = now;

                    Log.Info(this, "Start capture.");

                    //var tex = ((IosArService) _ar).Video.VideoY;
                    //_worker.Enqueue(tex.GetPixels32(), tex.width, tex.height);

                    /*
                    _grabber
                        .Grab()
                        .OnSuccess(texture =>
                        {
                            Log.Info(this, "Grabber got! Queue work.");
                            
                            _colors = texture.GetPixels32();
                    
                            _worker.Enqueue(
                                _colors,
                                texture.width,
                                texture.height);
                        });*/
                }

                yield return null;
            }
            
            Log.Info(this, "Exit poll.");
        }

        /// <summary>
        /// Called when the decoder has decoded something. This is guaranteed to
        /// be called on the main thread.
        /// </summary>
        /// <param name="id">Id of the capture.</param>
        /// <param name="value">Value.</param>
        private void Decoder_OnDecoded(int id, string value)
        {
            Log.Info(this, "Read : {0}.", value);
            
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
            Log.Info(this, "Failed to read capture {0}.", id);
        }
    }
}
#endif