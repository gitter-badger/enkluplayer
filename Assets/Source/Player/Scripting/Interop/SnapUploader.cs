using CreateAR.EnkluPlayer;

namespace Source.Player.Scripting.Interop
{
    /// <summary>
    /// Uploader interface for snaps.
    /// </summary>
    public class SnapUploader
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private IVideoManager _videoManager;
        private IImageManager _imageManager;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public SnapUploader(IVideoManager videoManager, IImageManager imageManager)
        {
            _videoManager = videoManager;
            _imageManager = imageManager;
        }

        /// <summary>
        /// Enables the uploaders for processing.
        /// </summary>
        public void enableUploads(string tag, bool uploadExisting = false)
        {
            _videoManager.EnableUploads(tag, uploadExisting);
            _imageManager.EnableUploads(tag, uploadExisting);
        }

        /// <summary>
        /// Disables the uploaders from processing.
        /// </summary>
        public void disableUploads()
        {
            _videoManager.DisableUploads();
            _imageManager.DisableUploads();
        }
    }
}