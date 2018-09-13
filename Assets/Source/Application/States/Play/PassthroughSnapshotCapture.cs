using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Passthrough implementation.
    /// </summary>
    public class PassthroughSnapshotCapture : ISnapshotCapture
    {
        /// <inheritdoc />
        public void Setup()
        {
            //
        }

        /// <inheritdoc />
        public void Capture()
        {
            Log.Info(this, "Passthrough snapshot capture.");
        }

        /// <inheritdoc />
        public void Capture(int width, int height)
        {
            Log.Info(this, "Passthrough snapshot capture.");
        }

        /// <inheritdoc />
        public void Teardown()
        {
            
        }
    }
}