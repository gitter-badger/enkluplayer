namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Passthrough implementation of IVideoManager and IImageManager.
    /// </summary>
    public class PassthroughMediaManager : IVideoManager, IImageManager
    {
        /// <summary>
        /// Enabled uploads.
        /// </summary>
        public void EnableUploads(string tag, bool uploadExisting = false)
        {
        }

        /// <summary>
        /// Disables uploads.
        /// </summary>
        public void DisableUploads()
        {
        }
    }
}