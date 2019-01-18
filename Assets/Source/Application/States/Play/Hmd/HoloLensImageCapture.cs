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
        
        /// <inheritdoc />
        public IAsyncToken<Void> Warm()
        {
            if (_photoCapture != null)
            {
                return new AsyncToken<Void>(new Exception("HoloLensImageCapture already warmed."));
            }
            
            var rtnToken = new AsyncToken<Void>();
            
            PhotoCapture.CreateAsync(true, captureObject =>
            {
                _photoCapture = captureObject;
                _photoCapture.StartPhotoModeAsync(_cameraParameters, result =>
                {
                    if (!result.success)
                    {
                        Abort();
                        rtnToken.Fail(new Exception(string.Format("Failure starting PhotoMode ({0})", result.hResult)));
                    }
                    else
                    {
                        Log.Info(this, "Entered PhotoMode.");
                        rtnToken.Succeed(Void.Instance);
                    }
                });
            });

            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<string> Capture()
        {
            IAsyncToken<Void> warmToken;
            
            if (_photoCapture == null)
            {
                warmToken = Warm();
            }
            else
            {
                warmToken = new AsyncToken<Void>(Void.Instance);
            }

            var rtnToken = new AsyncToken<string>();
            
            warmToken
                .OnSuccess(_ =>
                {
                    var filename = string.Format("{0:yyyy.MM.dd-HH.mm.ss}.png", DateTime.UtcNow);
                    var savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "images");
                    var fullName = Path.Combine(savePath, filename);
                    
                    Directory.CreateDirectory(savePath);
                    
                    _photoCapture.TakePhotoAsync(fullName, PhotoCaptureFileOutputFormat.PNG,
                        result =>
                        {
                            Abort();
                            
                            if (!result.success)
                            {
                                rtnToken.Fail(new Exception(string.Format("Error capturing image ({0})", result.hResult)));
                            }
                            else
                            {
                                rtnToken.Succeed(fullName);
                            }
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
            
            if (_photoCapture != null)
            {
                _photoCapture.StopPhotoModeAsync(result =>
                {
                    _photoCapture.Dispose();
                    _photoCapture = null;
                    
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