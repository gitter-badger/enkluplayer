namespace CreateAR.EnkluPlayer
{
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
        /// Captures a snapshot with the default/recommended resolution.
        /// </summary>
        void CaptureImage();

        /// <summary>
        /// Captures a snapshot with a specific resolution. <see cref="Capture"/>
        /// should probably be used to ensure the best quality per device.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void CaptureImage(int width, int height);

        void EnterVideoMode();

        void ExitVideoMode();

        void StartCaptureVideo();

        void StopCaptureVideo();

        /// <summary>
        /// Shuts down the capture.
        /// </summary>
        void Teardown();
    }
}