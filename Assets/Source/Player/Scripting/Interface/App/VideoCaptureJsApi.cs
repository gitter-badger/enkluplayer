
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Jint;
using Jint.Native;
using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

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
        /// <param name="engine"></param>
        /// <param name="callback"></param>
        public void setup(Engine engine, JsFunc callback)
        {
            JsCallback(engine, _videoCapture.Setup(), callback); 
        }

        /// <summary>
        /// Starts a recording.
        /// TODO: Remove this overload when EK-1124 is resolved.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="callback"></param>
        public void start(Engine engine, JsFunc callback)
        {
            startCustomPath(engine, callback, null);
        }
        
        /// <summary>
        /// Starts a recording.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="callback"></param>
        /// <param name="customPath"></param>
        public void startCustomPath(Engine engine, JsFunc callback, string customPath)
        {
            JsCallback(engine, _videoCapture.Start(customPath), callback); 
        }
        
        /// <summary>
        /// Stops a recording.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="callback"></param>
        public void stop(Engine engine, JsFunc callback)
        {
            JsCallback(engine, _videoCapture.Stop(), callback); 
        }
        
        /// <summary>
        /// Cancels the capture process.
        /// </summary>
        /// <param name="engine"></param>
        public void teardown(Engine engine)
        {
            _videoCapture.Teardown();
        }

        /// <summary>
        /// Helper to invoke a callback after a media capture operation if it exists.
        /// </summary>
        /// <param name="engine">Jint Engine</param>
        /// <param name="token">MediaCapture return token</param>
        /// <param name="callback">Js callback</param>
        private void JsCallback<T>(Engine engine, IAsyncToken<T> token, JsFunc callback)
        {
            if (callback != null)
            {
                token
                    .OnSuccess(value => callback(JsValue.FromObject(engine, this), new JsValue[] { true, JsValue.FromObject(engine, value) }))
                    .OnFailure(e =>
                    {
                        Log.Error(this, e);
                        callback(JsValue.FromObject(engine, this), new JsValue[] { false });
                    });
            }
        }
    }
}