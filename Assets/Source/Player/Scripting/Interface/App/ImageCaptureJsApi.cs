using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Jint;
using Jint.Native;
using JsFunc = System.Func<Jint.Native.JsValue, Jint.Native.JsValue[], Jint.Native.JsValue>;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class ImageCaptureJsApi
    {
        private readonly IImageCapture _imageCapture;

        public ImageCaptureJsApi(IImageCapture imageCapture)
        {
            _imageCapture = imageCapture;
        }
        
        public void warm(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _imageCapture.Warm(), callback);
        }

        public void capture(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, Async.Map(_imageCapture.Capture(), _ => Void.Instance) , callback);
        }

        public void abort(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _imageCapture.Abort(), callback);
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