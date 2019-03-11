using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Enklu.Orchid;

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
        /// <param name="callback"></param>
        public void warm(IJsCallback callback)
        {
            JsCallback(_imageCapture.Warm(), callback);
        }

        /// <summary>
        /// Captures an image.
        /// TODO: Remove this overload when EK-1124 is resolved.
        /// </summary>
        /// <param name="callback"></param>
        public void capture(IJsCallback callback)
        {
            captureCustomPath(callback, null);
        }

        /// <summary>
        /// Captures an image.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="customPath"></param>
        public void captureCustomPath(IJsCallback callback, string customPath)
        {
            JsCallback(_imageCapture.Capture(customPath) , callback);
        }

        /// <summary>
        /// Cancels the capture process.
        /// </summary>
        /// <param name="callback"></param>
        public void abort(IJsCallback callback)
        {
            JsCallback(_imageCapture.Abort(), callback);
        }

        /// <summary>
        /// Helper to invoke a callback after a media capture operation if it exists.
        /// </summary>
        /// <param name="token">MediaCapture return token</param>
        /// <param name="callback">Js callback</param>
        private void JsCallback<T>(IAsyncToken<T> token, IJsCallback callback)
        {
            if (callback != null)
            {
                token
                    .OnSuccess(value => callback.Apply(this, true, value))
                    .OnFailure(e =>
                    {
                        Log.Error(this, e);
                        callback.Apply(this, false);
                    });
            }
        }
    }
}