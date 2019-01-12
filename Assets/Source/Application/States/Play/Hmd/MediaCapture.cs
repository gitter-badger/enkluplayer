using CreateAR.Commons.Unity.Logging;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;

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
        public void Setup()
        {
            // TODO: if Teardown() is called before this returns, we need to destroy the
            // TODO: object
            PhotoCapture.CreateAsync(true, captureObject => _photoCapture = captureObject);
            VideoCapture.CreateAsync(true, captureObject => _videoCapture = captureObject);
        }

        /// <inheritdoc />
        public void CaptureImage()
        {
            CaptureImage(WIDTH, HEIGHT);
        }

        /// <inheritdoc />
        public void CaptureImage(int width, int height)
        {
            if (_photoCapture == null)
            {
                Log.Error(this, "PhotoCapture not created yet.");
                return;
            }
            
            Log.Info(this, string.Format("Starting image capture ({0}x{1}).", width, height));

            var cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 1.0f;
            cameraParameters.cameraResolutionWidth = width;
            cameraParameters.cameraResolutionHeight = height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            _photoCapture.StartPhotoModeAsync(cameraParameters, OnEnterPhotoMode);
        }

        public void EnterVideoMode()
        {
            if (_videoCapture == null)
            {
                Log.Error(this, "VideoCapture not created yet.");
                return;
            }
            
            var frameRate = VideoCapture.GetSupportedFrameRatesForResolution(new Resolution(){width = WIDTH, height = HEIGHT})
                .OrderByDescending((fps) => fps).First();
            
            Log.Info(this, "Starting video capture ({0}x{1}x{2})", WIDTH, HEIGHT, frameRate);

            var cameraParameters = new CameraParameters()
            {
                hologramOpacity = 1.0f,
                cameraResolutionWidth = WIDTH,
                cameraResolutionHeight = HEIGHT,
                frameRate = frameRate
            };
            
            _videoCapture.StartVideoModeAsync(cameraParameters, VideoCapture.AudioState.ApplicationAndMicAudio,
                (result) =>
                {
                    if (!result.success)
                    {
                        Log.Error(this, "Failed to start VideoMode ({0})", result.hResult);
                        EndVideoCapture();
                        return;
                    }
            
                    Log.Info(this, "Entered VideoMode");
                });
        }

        public void ExitVideoMode()
        {
            EndVideoCapture();
        }
        
        public void StartCaptureVideo()
        {
            var filename = string.Format("{0}.mp4", DateTime.UtcNow.ToString("yyyy.MM.dd-HH.mm.ss"));
            var savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "video");

            Directory.CreateDirectory(savePath);

            var fullPath = Path.Combine(savePath, filename);
            
            _videoCapture.StartRecordingAsync(fullPath, OnRecordingStart);
        }

        public void StopCaptureVideo()
        {
            EndVideoCapture();
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

        #region Image Capture
        /// <summary>
        /// Invoked after PhotoMode has attempted to enter, successfully or not.
        /// </summary>
        /// <param name="result">PhotoMode entry result.</param>
        private void OnEnterPhotoMode(PhotoCapture.PhotoCaptureResult result)
        {
            if (!result.success)
            {
                Log.Error(this, string.Format("Failed to start PhotoMode ({0}).", result.hResult));
                EndScreenshotCapture();
                return;
            }

            Log.Info(this, "Entered PhotoMode.");

            var filename = string.Format("{0}.png", DateTime.UtcNow.ToString("yyyy.MM.dd-HH.mm.ss"));
            var savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "snapshots");
            
            // Make sure "snapshots" dir exists
            Directory.CreateDirectory(savePath);

            var fullPath = Path.Combine(savePath, filename);

            // PhotoCapture API will handle saving the image directly to disk, avoiding coming back to Unity.
            _photoCapture.TakePhotoAsync(fullPath, PhotoCaptureFileOutputFormat.PNG, (takeResult) =>
            {
                OnTakePhoto(takeResult, fullPath, filename);
            });
        }

        /// <summary>
        /// Invoked after TakePhoto runs, successfully or not.
        /// </summary>
        /// <param name="result">TakePhoto result.</param>
        /// <param name="fullPath">Full path + name of the image.</param>
        /// <param name="filename">Filename of the image. (Used in debugging)</param>
        private void OnTakePhoto(PhotoCapture.PhotoCaptureResult result, string fullPath, string filename)
        {
            if (!result.success)
            {
                Log.Error(this, "Failed to take snapshot: " + result.hResult);
                EndScreenshotCapture();
                return;
            }

            Log.Info(this, "Snapshot saved to " + fullPath);

#if DEBUG_SNAPSHOTS && !UNITY_EDITOR
            // Copy the file to the device's camera roll so it can be accessed via USB easily.
            var cameraRollFolder = Windows.Storage.KnownFolders.CameraRoll.Path;
            var newPath = Path.Combine(cameraRollFolder, filename);
            File.Copy(fullPath, newPath);
            Log.Info(this, "Snapshot moved to " + newPath);
#endif

            EndScreenshotCapture();
        }

        /// <summary>
        /// Cleans up the PhotoCapture state, exiting PhotoMode if needed.
        /// </summary>
        private void EndScreenshotCapture()
        {
            _photoCapture.StopPhotoModeAsync((result) =>
            {
                if (result.success)
                {
                    Log.Info(this, "Exited PhotoMode.");
                }
                else
                {
                    Log.Error(this, string.Format("Failed to stop PhotoMode ({0}).", result.hResult));
                }
            });
        }
        #endregion
        
        #region Video Capture

        private void OnRecordingStart(VideoCapture.VideoCaptureResult result)
        {
            if (!result.success)
            {
                Log.Error(this, "Failed to start Recording ({0})", result.hResult);
                EndVideoCapture();
                return;
            }
            
            Log.Info(this, "Recording started.");
        }

        private void EndVideoCapture()
        {
            _videoCapture.StopRecordingAsync((recResult) =>
            {
                if (!recResult.success)
                {
                    Log.Error(this, "Failed to stop recording ({0)", recResult.hResult);
                }
                else
                {
                    Log.Info(this, "Recording stopped.");
                }
                
                _videoCapture.StopVideoModeAsync((modeResult) =>
                {
                    if (!modeResult.success)
                    {
                        Log.Error(this, "Failed to stop VideoMode ({0})", modeResult.hResult);
                    }
                });
            });
        }
        #endregion
    }
}