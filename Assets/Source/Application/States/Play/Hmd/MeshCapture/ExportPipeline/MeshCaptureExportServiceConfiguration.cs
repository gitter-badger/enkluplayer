namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Configures pipeline.
    /// </summary>
    public class MeshCaptureExportServiceConfiguration
    {
        /// <summary>
        /// Timeout for trying to queue a scan.
        /// </summary>
        public int LockTimeoutMs = 3;

        /// <summary>
        /// Maximum length of the scan queue.
        /// </summary>
        public int MaxScanQueueLen = 1;
    }
}