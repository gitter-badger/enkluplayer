using CreateAR.Commons.Unity.Logging;
using System;
using System.IO;
using UnityEngine.XR.WSA.WebCam;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Listens to a voice command, and invokes the full flow of saving snapshots to disk.
    /// 
    /// Useful documentation: https://docs.microsoft.com/en-us/windows/mixed-reality/locatable-camera
    /// Currently defaults to 1280x720 to avoid cropping holograms.
    /// </summary>
    public class SnapshotCapture : ISnapshotCapture
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

        public SnapshotCapture()
        {
            PhotoCapture.CreateAsync(true, (PhotoCapture captureObject) =>
            {
                _photoCapture = captureObject;
            });
        }

        /// <summary>
        /// Creates a snapshot with the default Hololens resolution.
        /// </summary>
        public void Capture()
        {
            Capture(WIDTH, HEIGHT);
        }

        /// <summary>
        /// Creates a snapshot with a specified resolution.
        /// </summary>
        /// <param name="command">Voice command string.</param>
        public void Capture(int width, int height)
        {
            if (_photoCapture == null)
            {
                Log.Error(this, "PhotoCapture not created yet.");
                return;
            }

            // TODO: Capture lower resolution to avoid holograms visually clipping?
            Log.Info(this, string.Format("Starting snapshot capture ({0}x{1}).", WIDTH, HEIGHT));

            var cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 1.0f;
            cameraParameters.cameraResolutionWidth = WIDTH;
            cameraParameters.cameraResolutionHeight = HEIGHT;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            _photoCapture.StartPhotoModeAsync(cameraParameters, OnEnterPhotoMode);
        }

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
            var fullPath = Path.Combine(UnityEngine.Application.persistentDataPath, filename);
            
            // Windows API will handle saving the image directly to disk, avoiding coming back to Unity.
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

        /// <summary>
        /// Disposes of the underlying PhotoCapture instance.
        /// </summary>
        public void Teardown()
        {
            _photoCapture.Dispose();
            _photoCapture = null;
        }
    }
}
