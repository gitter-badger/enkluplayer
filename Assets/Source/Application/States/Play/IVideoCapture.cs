using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Used to capture videos from a device during runtime.
    /// </summary>
    public interface IVideoCapture
    {
        /// <summary>
        /// Returns whether the device is currently recording a video or not.
        /// </summary>
        bool IsRecording { get; }
        
        /// <summary>
        /// Warms the video capture system. This may yield faster Start() calls depending on the device.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Warm();

        /// <summary>
        /// Starts recording a video.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Start();

        /// <summary>
        /// Stops recording a video. File save path is returned.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<string> Stop();

        /// <summary>
        /// Aborts the video capture and frees underlying resources.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Abort();
        
    }
}