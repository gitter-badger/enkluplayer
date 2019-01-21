using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Jint;
using Jint.Native;
using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Scripting interface for capturing Images.
    /// </summary>
    public class ImageCaptureJsApi
    {
        /// <summary>
        /// Backing IImageCapture system.
        /// </summary>
        private readonly IImageCapture _imageCapture;

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageCaptureJsApi(IImageCapture imageCapture)
        {
            _imageCapture = imageCapture;
        }
        
        /// <summary>
        /// Preps the image capture system. Optional.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="callback"></param>
        public void warm(Engine engine, JsFunc callback)
        {
            JsCallback(engine, _imageCapture.Warm(), callback);
        }

        /// <summary>
        /// Captures an image.
        /// TODO: Remove this overload when EK-1124 is resolved.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="callback"></param>
        public void capture(Engine engine, JsFunc callback)
        {
            capture(engine, callback, null);
        }
        
        /// <summary>
        /// Captures an image.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="callback"></param>
        /// <param name="customPath"></param>
        public void capture(Engine engine, JsFunc callback, string customPath)
        {
            JsCallback(engine, _imageCapture.Capture(customPath) , callback);
        }

        /// <summary>
        /// Cancels the capture process.
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="callback"></param>
        public void abort(Engine engine, JsFunc callback)
        {
            JsCallback(engine, _imageCapture.Abort(), callback);
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