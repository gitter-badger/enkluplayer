using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class PassthroughSnapshotCapture : ISnapshotCapture
    {
        /// <summary>
        /// Passthrough capture.
        /// </summary>
        public void Capture()
        {
            Log.Info(this, "Passthrough snapshot capture.");
        }

        /// <summary>
        /// Passthrough capture.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Capture(int width, int height)
        {
            Log.Info(this, "Passthrough snapshot capture.");
        }

        /// <summary>
        /// Passthrough teardown.
        /// </summary>
        public void Teardown()
        {
            
        }
    }
}