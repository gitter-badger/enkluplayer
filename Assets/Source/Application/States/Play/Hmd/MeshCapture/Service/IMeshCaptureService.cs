namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Service that captures mesh from environment.
    /// </summary>
    public interface IMeshCaptureService
    {
        /// <summary>
        /// True iff mesh captures are visible. Defaults to false.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Observer that receives updates.
        /// </summary>
        IMeshCaptureObserver Observer { get; set; }

        /// <summary>
        /// Starts capture and pushes updates through passed in observer.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops capture.
        /// </summary>
        void Stop();
    }
}