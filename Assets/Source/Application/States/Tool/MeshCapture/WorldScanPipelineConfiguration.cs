namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Configures pipeline.
    /// </summary>
    public class WorldScanPipelineConfiguration
    {
        /// <summary>
        /// Timeout for trying to queue a scan.
        /// </summary>
        public int LockTimeoutMs = 3;

        /// <summary>
        /// Maximum length of the scan queue.
        /// </summary>
        public int MaxScanQueueLen = 3;

        /// <summary>
        /// Maximum allowed on disk at once.
        /// </summary>
        public int MaxOnDisk = 10;
    }
}