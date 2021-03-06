﻿namespace CreateAR.EnkluPlayer
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
        /// True iff Start has been called without a Stop.
        /// </summary>
        bool IsRunning { get; }

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