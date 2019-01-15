using CreateAR.Commons.Unity.Logging;
using System;
using System.IO;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Records images and video from the HoloLens, as a mix of the device's front camera & holograms.
    /// Useful documentation: https://docs.microsoft.com/en-us/windows/mixed-reality/locatable-camera
    /// Currently defaults to 1280x720 to avoid cropping holograms.
    /// </summary>
    public class MediaCapture : IMediaCapture
    {
        /// <summary>
        /// Snapshot width.
        /// </summary>
        private const int WIDTH = 1280;

        /// <summary>
        /// Snapshot height.
        /// </summary>
        private const int HEIGHT = 720;
        
        /// <summary>
        /// Entrypoint to the PhotoMode API.
        /// </summary>
        private PhotoCapture _photoCapture;
        
        /// <summary>
        /// Entrypoint to the VideoCapture API.
        /// </summary>
        private VideoCapture _videoCapture;
        
        /// <inheritdoc />
        public CaptureState CaptureState { get; private set; }

        /// <inheritdoc />
        public void Setup()
        {
            // TODO: if Teardown() is called before this returns, we need to destroy the
            // TODO: object
            PhotoCapture.CreateAsync(true, captureObject => _photoCapture = captureObject);
            VideoCapture.CreateAsync(true, captureObject => _videoCapture = captureObject);

            CaptureState = CaptureState.Idle;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> EnterPhotoMode()
        {
            if (_photoCapture == null)
            {
                return new AsyncToken<Void>(new Exception("PhotoCapture not created yet."));
            }

            if (CaptureState != CaptureState.Idle)
            {
                return new AsyncToken<Void>(new Exception(string.Format("MediaCapture is not idle ({0})", CaptureState)));
            }
            
            Log.Info(this, "Starting PhotoMode ({0}x{1})", WIDTH, HEIGHT);
            
            var cameraParameters = new CameraParameters
            {
                hologramOpacity = 1.0f,
                cameraResolutionWidth = WIDTH,
                cameraResolutionHeight = HEIGHT,
                pixelFormat = CapturePixelFormat.BGRA32
            };

            var rtnToken = new AsyncToken<Void>();
            
            _photoCapture.StartPhotoModeAsync(cameraParameters, result =>
            {
                if (!result.success)
                {
                    ExitPhotoMode();
                    rtnToken.Fail(new Exception(string.Format("Failed to start PhotoMode ({0})", result.hResult)));
                    return;
                }
        
                Log.Info(this, "Entered PhotoMode");
                CaptureState = CaptureState.PhotoMode;
                
                rtnToken.Succeed(Void.Instance);
            });

            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> CaptureImage()
        {
            var filename = string.Format("{0:yyyy.MM.dd-HH.mm.ss}.png", DateTime.UtcNow);
            var savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "snapshots");
            var fullPath = Path.Combine(savePath, filename);
            
            Directory.CreateDirectory(savePath);

            var rtnToken = new AsyncToken<Void>();
            
            _photoCapture.TakePhotoAsync(fullPath, PhotoCaptureFileOutputFormat.PNG, (result) =>
            {
                if (!result.success)
                {
                    ExitPhotoMode();
                    rtnToken.Fail(new Exception(string.Format("Failed to take snapshot: " + result.hResult)));
                    return;
                }

                Log.Info(this, "Snapshot saved to " + fullPath);
                rtnToken.Succeed(Void.Instance);
            });

            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> ExitPhotoMode()
        {
            var rtnToken = new AsyncToken<Void>();
            
            _photoCapture.StopPhotoModeAsync(result =>
            {
                if (!result.success)
                {
                    rtnToken.Fail(new Exception(string.Format("Failed to stop PhotoMode ({0})", result.hResult)));
                }
                else
                {
                    Log.Info(this, "Exited PhotoMode.");
                    CaptureState = CaptureState.Idle;
                    rtnToken.Succeed(Void.Instance);
                }
            });

            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> EnterVideoMode()
        {
            if (_videoCapture == null)
            {
                return new AsyncToken<Void>(new Exception("VideoCapture not created yet."));
            }

            if (CaptureState != CaptureState.Idle)
            {
                return new AsyncToken<Void>(new Exception(string.Format("MediaCapture is not idle ({0})", CaptureState)));
            }
            
            var frameRate = VideoCapture.GetSupportedFrameRatesForResolution(new Resolution(){width = WIDTH, height = HEIGHT})
                .OrderByDescending((fps) => fps).First();
            
            Log.Info(this, "Starting VideoMode ({0}x{1}x{2})", WIDTH, HEIGHT, frameRate);

            var cameraParameters = new CameraParameters()
            {
                hologramOpacity = 1.0f,
                cameraResolutionWidth = WIDTH,
                cameraResolutionHeight = HEIGHT,
                frameRate = frameRate
            };
            
            var rtnToken = new AsyncToken<Void>();
            
            _videoCapture.StartVideoModeAsync(cameraParameters, VideoCapture.AudioState.ApplicationAndMicAudio, (result) =>
            {
                if (!result.success)
                {
                    EndVideoCapture();
                    rtnToken.Fail(new Exception(string.Format("Failed to start VideoMode ({0})", result.hResult)));
                    return;
                }
        
                Log.Info(this, "Entered VideoMode");
                CaptureState = CaptureState.VideoMode;
                
                rtnToken.Succeed(Void.Instance);
            });
            
            return rtnToken;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> StartRecording()
        {
            var filename = string.Format("{0:yyyy.MM.dd-HH.mm.ss}.mp4", DateTime.UtcNow);
            var savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "video");

            Directory.CreateDirectory(savePath);

            var fullPath = Path.Combine(savePath, filename);
            
            var rtnToken = new AsyncToken<Void>();
            
            _videoCapture.StartRecordingAsync(fullPath, result =>
            {
                if (!result.success)
                {
                    EndVideoCapture();
                    rtnToken.Fail(new Exception(string.Format("Failed to start Recording ({0})", result.hResult)));
                    return;
                }

                CaptureState = CaptureState.VideoRecording;
                Log.Info(this, "Recording started.");
                rtnToken.Succeed(Void.Instance);
            });

            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> StopRecording()
        {
            var rtnToken = new AsyncToken<Void>();
            
            _videoCapture.StopRecordingAsync((recResult) =>
            {
                if (!recResult.success)
                {
                    rtnToken.Fail(new Exception(string.Format("Failed to stop recording ({0})", recResult.hResult)));
                }
                else
                {
                    CaptureState = CaptureState.VideoMode;
                    Log.Info(this, "Recording stopped.");
                    rtnToken.Succeed(Void.Instance);
                }
            });

            return rtnToken;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> ExitVideoMode()
        {
            return EndVideoCapture();
        }

        /// <inheritdoc />
        public void Teardown()
        {
            if (null != _photoCapture)
            {
                _photoCapture.Dispose();
                _photoCapture = null;
            }

            if (null != _videoCapture)
            {
                _videoCapture.Dispose();
                _videoCapture = null;
            }
        }
        
        /// <summary>
        /// Helper to ensure any active recordings are finished before exiting VideoMode.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> EndVideoCapture()
        {
            var rtnToken = new AsyncToken<Void>();
            
            if (CaptureState == CaptureState.VideoRecording)
            {
                StopRecording();
            }

            _videoCapture.StopVideoModeAsync((modeResult) =>
            {
                if (!modeResult.success)
                {
                    rtnToken.Fail(new Exception(string.Format("Failed to stop VideoMode ({0})", modeResult.hResult)));
                }
                else
                {
                    CaptureState = CaptureState.Idle;
                    Log.Info(this, "Exited VideoMode.");
                    rtnToken.Succeed(Void.Instance);
                }
            });

            return rtnToken;
        }
    }
}