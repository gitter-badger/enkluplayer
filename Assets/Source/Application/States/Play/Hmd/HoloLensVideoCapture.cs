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
    /// A HoloLens implementation of IVideoCapture.
    /// Calling Warm() can reduce time-to-record delay.
    /// </summary>
    public class HoloLensVideoCapture : IVideoCapture
    {
        /// <summary>
        /// The Video capture state.
        /// </summary>
        private enum CaptureState
        {
            /// <summary>
            /// Idle. Nothing exists or is allocated yet.
            /// </summary>
            Idle,
            
            /// <summary>
            /// Creating a VideoCapture instance.
            /// </summary>
            SettingUp,
            
            /// <summary>
            /// Entering VideoMode.
            /// </summary>
            EnteringVideoMode,
            
            /// <summary>
            /// Currently in VideoMode.
            /// </summary>
            InVideoMode,
            
            /// <summary>
            /// Starting to record.
            /// </summary>
            StartingRecord,
            
            /// <summary>
            /// Currently recording.
            /// </summary>
            Recording,
            
            /// <summary>
            /// Stopping a recording.
            /// </summary>
            ExitingRecord,
            
            /// <summary>
            /// Existing VideoMode.
            /// </summary>
            ExitingVideoMode,
            
            /// <summary>
            /// An error occured in an Async VideoCapture call.
            /// </summary>
            Error
        }

        /// <summary>
        /// Metrics.
        /// </summary>
        private IMetricsService _metrics;

        /// <summary>
        /// Configuration for snaps.
        /// </summary>
        private SnapConfig _snapConfig;
        
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

        /// <summary>
        /// Current state of the capture process.
        /// </summary>
        private CaptureState _currentState;
        
        /// <inheritdoc />
        public Action<string> OnVideoCreated { get; set; }
        
        /// <inheritdoc />
        public bool IsRecording
        {
            get { return _videoCapture != null && _videoCapture.IsRecording; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public HoloLensVideoCapture(IMetricsService metrics, ApplicationConfig applicationConfig)
        {
            _metrics = metrics;
            _snapConfig = applicationConfig.Snap;

            _currentState = CaptureState.Idle;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Setup(VideoAudioSource videoAudioSource = VideoAudioSource.Mixed)
        {
            if (_currentState != CaptureState.Idle)
            {
                return new AsyncToken<Void>(new Exception(string.Format("Video CaptureState not Idle ({0})",
                    _currentState)));
            }

            _currentState = CaptureState.SettingUp;

            var rtnToken = new AsyncToken<Void>();

            VideoCapture.CreateAsync(true, captureObject =>
            {
                if (_currentState != CaptureState.SettingUp)
                {
                    captureObject.Dispose();
                    return;
                }

                _videoCapture = captureObject;
                _currentState = CaptureState.EnteringVideoMode;

                var unityAudioState = VideoCapture.AudioState.ApplicationAndMicAudio;
                switch (videoAudioSource)
                {
                    case VideoAudioSource.Experience:
                        unityAudioState = VideoCapture.AudioState.ApplicationAudio;
                        break;
                    case VideoAudioSource.Mic:
                        unityAudioState = VideoCapture.AudioState.MicAudio;    
                        break;
                    case VideoAudioSource.Mixed:
                        unityAudioState = VideoCapture.AudioState.ApplicationAndMicAudio;    
                        break;
                    case VideoAudioSource.None:
                        unityAudioState = VideoCapture.AudioState.None;
                        break;
                }

                _videoCapture.StartVideoModeAsync(_cameraParameters, unityAudioState,
                    result =>
                    {
                        if (result.success)
                        {
                            Log.Info(this, "Entered VideoMode.");
                            _currentState = CaptureState.InVideoMode;
                            rtnToken.Succeed(Void.Instance);
                        }
                        else
                        {
                            var exception = new Exception(string.Format("Failure entering VideoMode ({0})", result.hResult));
                            Log.Error(this, exception);
                            _currentState = CaptureState.Error;
                            rtnToken.Fail(exception);
                        }
                    });
            });

            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Start(string customPath = null)
        {
            if (_currentState != CaptureState.InVideoMode)
            {
                return new AsyncToken<Void>(new Exception(string.Format("Video CaptureState not InVideoMode ({0})", _currentState)));
            }
            
            var rtnToken = new AsyncToken<Void>();
            
            // Metrics
            _metrics.Value(MetricsKeys.MEDIA_VIDEO_START).Value(1);
            rtnToken
                .OnFailure(_ => _metrics.Value(MetricsKeys.MEDIA_VIDEO_FAILURE).Value(1));
            
            
            var filename = !string.IsNullOrEmpty(customPath) 
                ? customPath 
                : string.Format("{0:yyyy.MM.dd-HH.mm.ss}.mp4", DateTime.UtcNow);
                    
            // Don't rely on the user to supply the extension
            if (!filename.EndsWith(".mp4")) filename += ".mp4";
                    
            var videoFolder = Path.Combine(UnityEngine.Application.persistentDataPath, _snapConfig.VideoFolder);
            _recordingFilePath = Path.Combine(videoFolder, filename).Replace("/", "\\");
                    
            // Make sure to handle user paths (customPath="myExperienceName/myAwesomeVideo.mp4")
            Directory.CreateDirectory(_recordingFilePath.Substring(0, _recordingFilePath.LastIndexOf("\\")));
                    
            Log.Info(this, "Recording to: " + _recordingFilePath);
            _currentState = CaptureState.StartingRecord;
            _videoCapture.StartRecordingAsync(_recordingFilePath, result =>
            {
                if (result.success)
                {
                    _currentState = CaptureState.Recording;
                    rtnToken.Succeed(Void.Instance);
                }
                else
                {
                    _currentState = CaptureState.Error;
                    var exception = new Exception(string.Format("Failure starting recording ({0})", result.hResult));
                    Log.Error(this, exception);
                    rtnToken.Fail(exception);
                }
            });

            return rtnToken;
        }

        /// <inheritdoc />
        public IAsyncToken<string> Stop()
        {
            if (_currentState != CaptureState.Recording)
            {
                return new AsyncToken<string>(new Exception(string.Format("Video CaptureState not Recording ({0})", _currentState)));
            }
            
            var rtnToken = new AsyncToken<string>();
            
            // Metrics
            rtnToken
                .OnSuccess(_ => _metrics.Value(MetricsKeys.MEDIA_VIDEO_SUCCESS).Value(1))
                .OnFailure(_ => _metrics.Value(MetricsKeys.MEDIA_VIDEO_FAILURE).Value(1));

            _currentState = CaptureState.ExitingRecord;
            _videoCapture.StopRecordingAsync(result =>
            {   
                if (result.success)
                {
                    _currentState = CaptureState.InVideoMode;

                    RefreshVideoMode()
                        .OnSuccess(_ => rtnToken.Succeed(_recordingFilePath))
                        .OnFailure(exception =>
                        {
                            _currentState = CaptureState.Error;
                            rtnToken.Fail(exception);
                        });
                    
                    if (OnVideoCreated != null)
                    {
                        OnVideoCreated(_recordingFilePath);
                    }
                }
                else
                {
                    _currentState = CaptureState.Error;
                    var exception = new Exception(string.Format("Failure stopping recording ({0})", result.hResult));
                    Log.Error(this, exception);
                    rtnToken.Fail(exception);
                }
            });
            
            return rtnToken;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            if (_currentState == CaptureState.Idle)
            {
                return;
            }

            if (_currentState == CaptureState.Error)
            {
                Log.Error(this, "Teardown from error state?! Cross your fingers!");
            }

            // Possible if Setup() and Teardown() are called back to back.
            if (_videoCapture != null)
            {
                _currentState = CaptureState.ExitingVideoMode;
                
                _videoCapture.StopVideoModeAsync(result =>
                {
                    _videoCapture.Dispose();
                    _videoCapture = null;
                    
                    if (result.success)
                    {
                        _currentState = CaptureState.Idle;
                        Log.Info(this, "Exited VideoMode");
                    }
                    else
                    {
                        _currentState = CaptureState.Error;
                        throw new Exception(string.Format("Failure exiting VideoMode ({0})", result.hResult));
                    }
                });
            }
            else
            {
                _currentState = CaptureState.Idle;
            }
        }

        /// <summary>
        /// To get around a video capture bug, VideoMode must exit/enter after each capture.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> RefreshVideoMode()
        {
            var rtnToken = new AsyncToken<Void>();
            
            Log.Info(this, "Refreshing VideoMode.");
            if (_videoCapture != null)
            {
                _videoCapture.StopVideoModeAsync(stopResult =>
                {
                    if (stopResult.success)
                    {
                        if (_videoCapture != null)
                        {
                            _videoCapture.StartVideoModeAsync(_cameraParameters, VideoCapture.AudioState.ApplicationAndMicAudio, startResult =>
                            {
                                if (startResult.success)
                                {
                                    Log.Info(this, "VideoMode Refreshed.");
                                    rtnToken.Succeed(Void.Instance);
                                }
                                else
                                {
                                    var exception = new Exception(string.Format("Failure entering VideoMode ({0})", startResult.hResult));
                                    Log.Error(this, exception);
                                    rtnToken.Fail(exception);     
                                }
                            });
                        }
                    }
                    else
                    {
                        var exception = new Exception(string.Format("Failure entering VideoMode ({0})", stopResult.hResult));
                        Log.Error(this, exception);
                        rtnToken.Fail(exception);
                    }
                });
            }
            

            return rtnToken;
        }
    }
}

#endif