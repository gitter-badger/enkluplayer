#if UNITY_WSA

using System;
using System.Collections;
using System.IO;
using System.Threading;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine.XR.WSA.WebCam;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// A HoloLens implementation of IVideoCapture.
    /// Calling Warm() can reduce time-to-record delay.
    /// </summary>
    public class HoloLensVideoCapture : IVideoCapture
    {
        /// <summary>
        /// IBootstrapper.
        /// </summary>
        private IBootstrapper _bootstrapper;
        
        /// <summary>
        /// Underlying VideoCapture API.
        /// </summary>
        private VideoCapture _videoCapture;
        
        /// <summary>
        /// The camera params videos use. Hardcoded for now.
        /// </summary>
        private readonly CameraParameters _cameraParameters = new CameraParameters
        {
            hologramOpacity = 1.0f,
            cameraResolutionWidth = 1280,
            cameraResolutionHeight = 720,
            frameRate = 30
        };

        /// <summary>
        /// The path the video file is streaming to.
        /// </summary>
        private string _recordingFilePath;

        /// <summary>
        /// Cached token for Warm() invokes.
        /// </summary>
        private AsyncToken<Void> _warmToken;
        
        /// <inheritdoc />
        public bool IsRecording
        {
            get { return _videoCapture != null && _videoCapture.IsRecording; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public HoloLensVideoCapture(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Warm()
        {
            if (_warmToken != null)
            {
                return _warmToken;
            }
            
            _warmToken = new AsyncToken<Void>();
            
            VideoCapture.CreateAsync(true, captureObject =>
            {
                // Check to make sure Abort() wasn't called immediately after Warm()
                if (_warmToken == null)
                {
                    captureObject.Dispose();
                    return;
                }
                
                _videoCapture = captureObject;
                
                _videoCapture.StartVideoModeAsync(_cameraParameters, VideoCapture.AudioState.ApplicationAndMicAudio,
                    result =>
                    {
                        if (!result.success)
                        {
                            _bootstrapper.BootstrapCoroutine(FailToken(
                                _warmToken, 
                                new Exception(string.Format("Failure entering VideoMode ({0})", result.hResult))));
                            Abort();
                        }
                        else
                        {
                            Log.Info(this, "Entered VideoMode.");
                            _bootstrapper.BootstrapCoroutine(SucceedToken(_warmToken, Void.Instance));
                        }
                    });
            });

            return _warmToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start(string customPath = null)
        {
            var rtnToken = new AsyncToken<Void>();
            
            Warm()
                .OnSuccess(_ =>
                {
                    var filename = !string.IsNullOrEmpty(customPath) 
                        ? customPath 
                        : string.Format("{0:yyyy.MM.dd-HH.mm.ss}.mp4", DateTime.UtcNow);
                    
                    // Don't rely on the user to supply the extension
                    if (!filename.EndsWith(".mp4")) filename += ".mp4";
                    
                    var videoFolder = Path.Combine(UnityEngine.Application.persistentDataPath, "videos");
                    _recordingFilePath = Path.Combine(videoFolder, filename).Replace("/", "\\");
                    
                    // Make sure to handle user paths (customPath="myExperienceName/myAwesomeVideo.mp4")
                    Directory.CreateDirectory(_recordingFilePath.Substring(0, _recordingFilePath.LastIndexOf("\\")));
                    
                    Log.Info(this, "Recording to: " + _recordingFilePath);
                    _videoCapture.StartRecordingAsync(_recordingFilePath, result =>
                    {
                        if (!result.success)
                        {
                            _bootstrapper.BootstrapCoroutine(FailToken(
                                rtnToken,
                                new Exception(string.Format("Failure starting recording ({0})", result.hResult))));
                            Abort();
                        }
                        else
                        {
                            _bootstrapper.BootstrapCoroutine(SucceedToken(rtnToken, Void.Instance));
                        }
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
        public IAsyncToken<string> Stop()
        {
            if (_warmToken == null)
            {
                return new AsyncToken<string>(new Exception("Video Warm() wasn't called."));
            }
            
            var rtnToken = new AsyncToken<string>();

            _warmToken.OnSuccess(_ =>
            {
                _videoCapture.StopRecordingAsync(result =>
                {   
                    // Abort regardless of success
                    Abort().OnFinally(__ =>
                    {
                        if (!result.success)
                        {
                            _bootstrapper.BootstrapCoroutine(FailToken(
                                rtnToken,
                                new Exception(string.Format("Failure stopping recording ({0})", result.hResult))));
                        }
                        else
                        {
                            _bootstrapper.BootstrapCoroutine(SucceedToken(rtnToken, _recordingFilePath));
                        }
                    });
                });
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

                var temp = _videoCapture;
                _videoCapture = null;
                
                temp.StopVideoModeAsync(result =>
                {
                    temp.Dispose();
                    
                    if (!result.success)
                    {
                        _bootstrapper.BootstrapCoroutine(FailToken(
                            rtnToken,
                            new Exception(string.Format("Failure exiting VideoMode ({0})", result.hResult))));
                    }
                    else
                    {
                        Log.Info(this, "Exited VideoMode");
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
    }
}

#endif