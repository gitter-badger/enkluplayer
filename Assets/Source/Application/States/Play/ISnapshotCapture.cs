namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Basic interface for capturing snapshots. 
    /// A snapshot is a image from a device with holograms rendered into it.
    /// </summary>
    public interface ISnapshotCapture
    {
        /// <summary>
        /// Captures a snapshot with the default/recommended resolution.
        /// </summary>
        void Capture();

        /// <summary>
        /// Captures a snapshot with a specific resolution. <see cref="Capture"/>
        /// should probably be used to ensure the best quality per device.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void Capture(int width, int height);

        /// <summary>
        /// Cleans up the ISnapshotCapture implementation.
        /// </summary>
        void Teardown();
    }
}