using CreateAR.EnkluPlayer;

namespace Source.Player.Scripting.Interop
{
    public class SnapUploader
    {
        private IVideoManager _videoManager;
        
        public SnapUploader(IVideoManager videoManager)
        {
            _videoManager = videoManager;
        }
        
        public void enableUploads(string tag)
        {
            _videoManager.EnableUploads(tag);
        }

        public void disableUploads()
        {
            _videoManager.DisableUploads();
        }
    }
}