
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using JsFunc = Enklu.Orchid.IJsCallback;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Scripting interface for capturing videos.
    /// </summary>
    public class VideoCaptureJsApi
    {
        /// <summary>
        /// Underlying IVideoCapture.
        /// </summary>
        private IVideoCapture _videoCapture;
        
        /// <summary>
        /// Returns whether the device is recording.
        /// </summary>
        public bool isRecording
        {
            get { return _videoCapture.IsRecording; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public VideoCaptureJsApi(IVideoCapture videoCapture)
        {
            _videoCapture = videoCapture;
        }

        /// <summary>
        /// Preps the video capture system. Optional.
        /// </summary>
        /// <param name="callback"></param>
        public void setup(JsFunc callback)
        {
            JsCallback(_videoCapture.Setup(), callback); 
        }

        /// <summary>
        /// Starts a recording.
        /// TODO: Remove this overload when EK-1124 is resolved.
        /// </summary>
        /// <param name="callback"></param>
        public void start(JsFunc callback)
        {
            startCustomPath(callback, null);
        }
        
        /// <summary>
        /// Starts a recording.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="customPath"></param>
        public void startCustomPath(JsFunc callback, string customPath)
        {
            JsCallback(_videoCapture.Start(customPath), callback); 
        }
        
        /// <summary>
        /// Stops a recording.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="callback"></param>
        public void stop(JsFunc callback)
        {
            JsCallback(_videoCapture.Stop(), callback); 
        }
        
        /// <summary>
        /// Cancels the capture process.
        /// </summary>
        /// <param name="engine"></param>
        public void teardown()
        {
            _videoCapture.Teardown();
        }

        /// <summary>
        /// Helper to invoke a callback after a media capture operation if it exists.
        /// </summary>
        /// <param name="token">MediaCapture return token</param>
        /// <param name="callback">Js callback</param>
        private void JsCallback<T>(IAsyncToken<T> token, JsFunc callback)
        {
            if (callback != null)
            {
                token
                    .OnSuccess(value => callback.Apply(this, true, value))
                    .OnFailure(e => callback.Apply(this, false));
            }
        }
    }
}