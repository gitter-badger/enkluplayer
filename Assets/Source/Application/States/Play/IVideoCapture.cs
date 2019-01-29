using System;
using CreateAR.Commons.Unity.Async;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Used to capture videos from a device during runtime.
    /// </summary>
    public interface IVideoCapture
    {
        /// <summary>
        /// Invoked whenever a new video has been created.
        /// </summary>
        Action<string> OnVideoCreated { get; set; } 
        
        /// <summary>
        /// Returns whether the device is currently recording a video or not.
        /// </summary>
        bool IsRecording { get; }
        
        /// <summary>
        /// Setups the device for video recording.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Setup();

        /// <summary>
        /// Starts recording a video.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Start(string customPath = null);

        /// <summary>
        /// Stops recording a video. File save path is returned.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<string> Stop();

        /// <summary>
        /// Cleans up any resources/state involved with video recording.
        /// </summary>
        /// <returns></returns>
        void Teardown();
    }
}