
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Jint;
using Jint.Native;
using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Allows Photos and Videos to be captured via scripting.
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

        public void warm(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _videoCapture.Warm(), callback); 
        }
        
        public void start(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _videoCapture.Start(), callback); 
        }
        
        public void stop(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, Async.Map(_videoCapture.Stop(), _ => Void.Instance), callback); 
        }
        
        public void abort(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _videoCapture.Abort(), callback);
        }

        /// <summary>
        /// Helper to invoke a callback after a media capture operation if it exists.
        /// </summary>
        /// <param name="engine">Jint Engine</param>
        /// <param name="token">MediaCapture return token</param>
        /// <param name="callback">Js callback</param>
        private void JsCallback(Engine engine, IAsyncToken<Void> token, JsFunc callback)
        {
            if (callback != null)
            {
                token
                    .OnSuccess(_ => callback(JsValue.FromObject(engine, this), new JsValue[] { true }))
                    .OnFailure(e =>
                    {
                        Log.Error(this, e);
                        callback(JsValue.FromObject(engine, this), new JsValue[] { false });
                    });
            }
        }
    }
}