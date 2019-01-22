#if UNITY_WSA

using System;
using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine.XR.WSA.WebCam;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// HoloLens implementation of IImage Capture.
    /// Calling Warm() can reduce time-to-capture delay.
    /// </summary>
    public class HoloLensImageCapture : IImageCapture
    {
        /// <summary>
        /// The underlying PhotoCapture API.
        /// </summary>
        private PhotoCapture _photoCapture;

        /// <summary>
        /// The camera parameters images will use. Hardcoded for now.
        /// </summary>
        private readonly CameraParameters _cameraParameters = new CameraParameters
        {
            hologramOpacity = 1.0f,
            cameraResolutionWidth = 1280,
            cameraResolutionHeight = 720
        };

        /// <summary>
        /// Cached token for Warm() invokes.
        /// </summary>
        private AsyncToken<Void> _warmToken;
        
        /// <inheritdoc />
        public IAsyncToken<Void> Warm()
        {
            if (_warmToken != null)
            {
                return _warmToken;
            }
            
            _warmToken = new AsyncToken<Void>();
            
            PhotoCapture.CreateAsync(true, captureObject =>
            {
                // Check to make sure Abort() wasn't called immediately after Warm()
                if (_warmToken == null)
                {
                    captureObject.Dispose();
                    return;
                }
                
                _photoCapture = captureObject;
                _photoCapture.StartPhotoModeAsync(_cameraParameters, result =>
                {
                    if (!result.success)
                    {
                        Abort();
                        _warmToken.Fail(new Exception(string.Format("Failure starting PhotoMode ({0})", result.hResult)));
                    }
                    else
                    {
                        Log.Info(this, "Entered PhotoMode.");
                        _warmToken.Succeed(Void.Instance);
                    }
                });
            });

            return _warmToken;
        }

        /// <inheritdoc />
        public IAsyncToken<string> Capture(string customPath = null)
        {
            var rtnToken = new AsyncToken<string>();

            Warm()
                .OnSuccess(_ =>
                {
                    var filename = !string.IsNullOrEmpty(customPath)
                        ? customPath
                        : string.Format("{0:yyyy.MM.dd-HH.mm.ss}.png", DateTime.UtcNow);
                    
                    // Don't rely on the user to supply the extension
                    if (!filename.EndsWith(".png")) filename += ".png";
                    
                    var savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "images");
                    var fullName = Path.Combine(savePath, filename).Replace("/", "\\");
    
                    // Make sure to handle user paths (customPath="myExperienceName/myAwesomeImage.png")
                    Directory.CreateDirectory(savePath.Substring(0, savePath.LastIndexOf("\\")));
    
                    Log.Info(this, "Saving image to: " + savePath);
                    _photoCapture.TakePhotoAsync(fullName, PhotoCaptureFileOutputFormat.PNG,
                        result =>
                        {
                            Abort().OnFinally(__ =>
                            {
                                if (!result.success)
                                {
                                    rtnToken.Fail(new Exception(string.Format("Error capturing image ({0})", result.hResult)));
                                }
                                else
                                {
                                    rtnToken.Succeed(fullName);
                                } 
                            });
                        });
                })
                .OnFailure(exception =>
                {
                    Abort();
                    rtnToken.Fail(exception);
                });

            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Abort()
        {
            var rtnToken = new AsyncToken<Void>();
            
            if (_warmToken != null)
            {
                _warmToken.Abort();
                _warmToken = null;
                
                var temp = _photoCapture;
                _photoCapture = null;
                
                temp.StopPhotoModeAsync(result =>
                {
                    temp.Dispose();
                    
                    if (!result.success)
                    {
                        rtnToken.Fail(new Exception(string.Format("Failure exiting PhotoMode ({0})", result.hResult)));
                    }
                    else
                    {
                        Log.Info(this, "Exited PhotoMode.");
                        rtnToken.Succeed(Void.Instance);
                    }
                });
            }
            else
            {
                rtnToken.Succeed(Void.Instance);
            }

            return rtnToken;
        }
    }
}

#endif