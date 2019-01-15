using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// The current state an IMediaCapture implementor is in.
    /// </summary>
    public enum CaptureState
    {
        /// <summary>
        /// Ready for anything.
        /// </summary>
        Idle,
            
        /// <summary>
        /// In PhotoMode, ready for instant snapshots.
        /// </summary>
        PhotoMode,
            
        /// <summary>
        /// In VideoMode, ready to start recording.
        /// </summary>
        VideoMode,
            
        /// <summary>
        /// Actively recording.
        /// </summary>
        VideoRecording
    }
    
    /// <summary>
    /// Basic interface for capturing video. 
    /// Videos captured contain the user's view with holograms rendered.
    /// </summary>
    public interface IMediaCapture
    {
        /// <summary>
        /// Prep for capture.
        /// </summary>
        void Setup();
        
        /// <summary>
        /// The current CaptureState.
        /// </summary>
        CaptureState CaptureState { get; }

        /// <summary>
        /// Enters PhotoMode.
        /// </summary>
        IAsyncToken<Void> EnterPhotoMode();

        /// <summary>
        /// Captures a snapshot with the default/recommended resolution.
        /// </summary>
        IAsyncToken<Void> CaptureImage();
        
        /// <summary>
        /// Exits PhotoMode.
        /// </summary>
        IAsyncToken<Void> ExitPhotoMode();

        /// <summary>
        /// Enters VideoMode.
        /// </summary>
        IAsyncToken<Void> EnterVideoMode();

        /// <summary>
        /// Starts a recording with the default/recommended resolution/framerate.
        /// </summary>
        IAsyncToken<Void> StartRecording();

        /// <summary>
        /// Stops an active recording.
        /// </summary>
        IAsyncToken<Void> StopRecording();
        
        /// <summary>
        /// Exits VideoMode.
        /// </summary>
        IAsyncToken<Void> ExitVideoMode();

        /// <summary>
        /// Shuts down the capture.
        /// </summary>
        void Teardown();
    }
}