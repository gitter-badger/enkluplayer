using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Used to capture images from a device during runtime.
    /// </summary>
    public interface IImageCapture
    {
        /// <summary>
        /// Invoked whenever a new image has been created.
        /// </summary>
        Action<string> OnImageCreated { get; set; }
        
        /// <summary>
        /// Warms the image capture system. This may yield faster Start() calls depending on the device.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Warm();

        /// <summary>
        /// Captures an image.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<string> Capture(string customPath = null);

        /// <summary>
        /// Aborts the image capture and frees underlying resources.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Abort();
    }
}