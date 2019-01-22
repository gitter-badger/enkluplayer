#if UNITY_WSA

using System;
using System.IO;
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
        private IHttpService _httpService;
        
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

        private AsyncToken<Void> _warmToken;
        
        /// <inheritdoc />
        public Action<string> OnVideoCreated { get; set; }
        
        /// <inheritdoc />
        public bool IsRecording
        {
            get { return _videoCapture != null && _videoCapture.IsRecording; }
        }

        public HoloLensVideoCapture(IHttpService httpService)
        {
            _httpService = httpService;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Warm()
        {
            Log.Info(this, "Warm");
            
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
                            _warmToken.Fail(new Exception(string.Format("Failure entering VideoMode ({0})", result.hResult)));
                            Abort();
                        }
                        else
                        {
                            Log.Info(this, "Entered VideoMode.");
                            _warmToken.Succeed(Void.Instance);
                        }
                    });
            });

            return _warmToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start(string customPath = null)
        {
            Log.Info(this, "Start");

            var rtnToken = new AsyncToken<Void>();
            
            Warm()
                .OnSuccess(_ =>
                {
                    var filename = !string.IsNullOrEmpty(customPath) ? customPath : string.Format("{0:yyyy.MM.dd-HH.mm.ss}.mp4", DateTime.UtcNow);
                    var savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "videos");
                    _recordingFilePath = Path.Combine(savePath, filename).Replace("/", "\\");
                    Log.Info(this, "Path: " + _recordingFilePath);
                    
                    Directory.CreateDirectory(savePath);
                    
                    _videoCapture.StartRecordingAsync(_recordingFilePath, result =>
                    {
                        if (!result.success)
                        {
                            rtnToken.Fail(new Exception(string.Format("Failure starting recording ({0})", result.hResult)));
                            Abort();
                        }
                        else
                        {
                            rtnToken.Succeed(Void.Instance);
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
        public IAsyncToken<string> Stop()
        {
            Log.Info(this, "Stop");
            
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
                        if (OnVideoCreated != null)
                        {
                            try
                            {
                                OnVideoCreated(_recordingFilePath);
                            }
                            catch (Exception e)
                            {
                                Log.Error(this, "Error invoking OnVideoCreated ({0})", e);
                            }
                            
                        }
                        
                        if (!result.success)
                        {
                            rtnToken.Fail(new Exception(string.Format("Failure stopping recording ({0})",
                                result.hResult)));
                        }
                        else
                        {
                            rtnToken.Succeed(_recordingFilePath);
                        }
                    });
                });
            });
            
            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Abort()
        {
            Log.Info(this, "Abort");
            
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
                        rtnToken.Fail(new Exception(string.Format("Failure exiting VideoMode ({0})", result.hResult)));
                    }
                    else
                    {
                        Log.Info(this, "Exited VideoMode");
                        
                        _videoCapture.Dispose();
                        _videoCapture = null;
                        
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