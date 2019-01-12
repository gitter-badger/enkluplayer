namespace CreateAR.EnkluPlayer.Scripting
{
    public class MediaCaptureJsApi
    {
        private IMediaCapture _mediaCapture;

        public MediaCaptureJsApi(IMediaCapture mediaCapture)
        {
            _mediaCapture = mediaCapture;
        }

        public void startRecording()
        {
            _mediaCapture.StartCaptureVideo();
        }

        public void stopRecording()
        {
            _mediaCapture.StopCaptureVideo();
        }

        public bool isRecording()
        {
            return true;
        }
    }
}