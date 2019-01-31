using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Video manager responsible for uploading/deleting videos on disk.
    /// </summary>
    public interface IVideoManager
    {
        /// <summary>
        /// Enables the uploader to be active.
        /// </summary>
        /// <param name="tag">The tag videos will be uploaded with.</param>
        /// <param name="uploadExisting">Whether files on disk should be uploaded.</param>
        void EnableUploads(string tag, bool uploadExisting = false);
        
        /// <summary>
        /// Disables the uploader. If an upload is in progress, it will finish.
        /// </summary>
        void DisableUploads();
    }
}