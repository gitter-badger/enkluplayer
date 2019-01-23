using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Used to capture images from a device during runtime.
    /// </summary>
    public interface IImageCapture
    {
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