#if UNITY_WSA

using System;
using System.Collections;
using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
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
        /// IBootstrapper.
        /// </summary>
        private IBootstrapper _bootstrapper;

        /// <summary>
        /// Configuration for snaps.
        /// </summary>
        private SnapConfig _snapConfig;
        
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
        public Action<string> OnImageCreated { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public HoloLensImageCapture(IBootstrapper bootstrapper, ApplicationConfig applicationConfig)
        {
            _bootstrapper = bootstrapper;
            _snapConfig = applicationConfig.Snap;
        }
        
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
                        _bootstrapper.BootstrapCoroutine(FailToken(
                            _warmToken,
                            new Exception(string.Format("Failure starting PhotoMode ({0})", result.hResult))));
                    }
                    else
                    {
                        Log.Info(this, "Entered PhotoMode.");
                        _bootstrapper.BootstrapCoroutine(SucceedToken(_warmToken, Void.Instance));
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
                    
                    var savePath = Path.Combine(UnityEngine.Application.persistentDataPath, _snapConfig.ImageFolder);
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
                                    _bootstrapper.BootstrapCoroutine(FailToken(
                                        rtnToken,
                                        new Exception(string.Format("Error capturing image ({0})", result.hResult))));
                                }
                                else
                                {
                                    if (OnImageCreated != null)
                                    {
                                        _bootstrapper.BootstrapCoroutine(ExecuteAction(OnImageCreated, fullName));
                                    }
                                    _bootstrapper.BootstrapCoroutine(SucceedToken(rtnToken, fullName));
                                } 
                            });
                        });
                })
                .OnFailure(exception =>
                {
                    Abort();
                    _bootstrapper.BootstrapCoroutine(FailToken(rtnToken, exception));
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
                        _bootstrapper.BootstrapCoroutine(FailToken(
                            rtnToken, 
                            new Exception(string.Format("Failure exiting PhotoMode ({0})", result.hResult))));
                    }
                    else
                    {
                        Log.Info(this, "Exited PhotoMode.");
                        _bootstrapper.BootstrapCoroutine(SucceedToken(rtnToken, Void.Instance));
                    }
                });
            }
            else
            {
                rtnToken.Succeed(Void.Instance);
            }

            return rtnToken;
        }
        
        /// <summary>
        /// Helper coroutine to succeed tokens on the main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SucceedToken<T>(AsyncToken<T> token, T value)
        {
            yield return null;
            token.Succeed(value);
        }

        /// <summary>
        /// Helper coroutine to fail tokens on the main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator FailToken<T>(AsyncToken<T> token, Exception exception)
        {
            yield return null;
            token.Fail(exception);
        }
        
        /// <summary>
        /// Helper coroutine to execute actions on the main thread. Assumes the token is not null.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecuteAction<T>(Action<T> action, T value)
        {
            yield return null;
            action(value);
        }
    }
}

#endif