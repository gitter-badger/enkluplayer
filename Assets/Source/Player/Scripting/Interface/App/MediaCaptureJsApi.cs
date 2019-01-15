
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
    public class MediaCaptureJsApi
    {
        /// <summary>
        /// Underlying IMediaCapture
        /// </summary>
        private IMediaCapture _mediaCapture;

        /// <summary>
        /// Returns whether the device is in PhotoMode.
        /// </summary>
        public bool inPhotoMode
        {
            get { return _mediaCapture.CaptureState == CaptureState.PhotoMode; }
        }

        /// <summary>
        /// Returns whether the device is in VideoMode.
        /// </summary>
        public bool inVideoMode
        {
            get { return _mediaCapture.CaptureState == CaptureState.VideoMode; }
        }
        
        /// <summary>
        /// Returns whether the device is recording.
        /// </summary>
        public bool isRecording
        {
            get { return _mediaCapture.CaptureState == CaptureState.VideoRecording; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mediaCapture"></param>
        public MediaCaptureJsApi(IMediaCapture mediaCapture)
        {
            _mediaCapture = mediaCapture;
        }

        /// <summary>
        /// Attempts to enter PhotoMode.
        /// </summary>
        public void enterPhotoMode(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _mediaCapture.EnterPhotoMode(), callback);
        }

        /// <summary>
        /// Attempts to capture an image.
        /// </summary>
        public void captureImage(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _mediaCapture.CaptureImage(), callback);
        }

        /// <summary>
        /// Attempts to exit PhotoMode.
        /// </summary>
        public void exitPhotoMode(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _mediaCapture.ExitPhotoMode(), callback);
        }
        
        /// <summary>
        /// Attempts to enter VideoMode.
        /// </summary>
        public void enterVideoMode(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _mediaCapture.EnterVideoMode(), callback);
        }
        
        /// <summary>
        /// Attempts to start a recording.
        /// </summary>
        public void startRecording(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _mediaCapture.StartRecording(), callback);
        }

        /// <summary>
        /// Attempts to stop a recording.
        /// </summary>
        public void stopRecording(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _mediaCapture.StopRecording(), callback);
        }
        
        /// <summary>
        /// Attempts to exit VideoMode.
        /// </summary>
        public void exitVideoMode(Engine engine, JsFunc callback = null)
        {
            JsCallback(engine, _mediaCapture.ExitVideoMode(), callback);
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

        /// <summary>
        /// Temp for debugging.
        /// </summary>
        public void rebuild()
        {
            _mediaCapture.Teardown();
            _mediaCapture.Setup();
        }
    }
}