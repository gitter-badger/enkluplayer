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
            
            if (_videoCapture != null)
            {
                return new AsyncToken<Void>(new Exception(("HoloLensVideoCapture already warmed.")));
            }
            
            var rtnToken = new AsyncToken<Void>();
            
            VideoCapture.CreateAsync(true, captureObject =>
            {
                _videoCapture = captureObject;
                
                _videoCapture.StartVideoModeAsync(_cameraParameters, VideoCapture.AudioState.ApplicationAndMicAudio,
                    result =>
                    {
                        if (!result.success)
                        {
                            rtnToken.Fail(new Exception(string.Format("Failure entering VideoMode ({0})", result.hResult)));
                            Abort();
                        }
                        else
                        {
                            Log.Info(this, "Entered VideoMode.");
                            rtnToken.Succeed(Void.Instance);
                        }
                    });
            });

            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start()
        {
            Log.Info(this, "Start");
            
            IAsyncToken<Void> warmToken;
            if (_videoCapture == null)
            {
                warmToken = Warm();
            }
            else
            {
                warmToken = new AsyncToken<Void>(Void.Instance);
            }
            
            var rtnToken = new AsyncToken<Void>();
            
            warmToken
                .OnSuccess(_ =>
                {
                    var filename = "testVideo.mp4"; //string.Format("{0:yyyy.MM.dd-HH.mm.ss}.mp4", DateTime.UtcNow);
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
            
            if (_videoCapture == null)
            {
                return new AsyncToken<string>(new Exception("Attempting to stop before starting recording."));
            }
            
            var rtnToken = new AsyncToken<string>();

            _videoCapture.StopRecordingAsync(result =>
            {   
                // Abort regardless of success
                Abort().OnFinally(_ =>
                {
                    if (!result.success)
                    {
                        rtnToken.Fail(new Exception(string.Format("Failure stopping recording ({0})",
                            result.hResult)));
                    }
                    else
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
                        rtnToken.Succeed(_recordingFilePath);
                    }
                });
            });
            
            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Abort()
        {
            Log.Info(this, "Abort");
            
            var rtnToken = new AsyncToken<Void>();
            
            if (_videoCapture != null)
            {
                _videoCapture.StopVideoModeAsync(result =>
                {
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