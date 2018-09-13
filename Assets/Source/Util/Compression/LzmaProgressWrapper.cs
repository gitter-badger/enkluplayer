using CreateAR.EnkluPlayer.Assets;
using SevenZip;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Forwards <c>ICodeProgress</c> to a <c>LoadProgress</c> object.
    /// </summary>
    internal class LzmaProgressWrapper : ICodeProgress
    {
        /// <summary>
        /// LoadProgress object.
        /// </summary>
        private readonly LoadProgress _progress;

        /// <summary>
        /// Total size of input.
        /// </summary>
        private readonly long _totalInSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LzmaProgressWrapper(
            LoadProgress progress,
            long totalInSize)
        {
            _progress = progress;
            _totalInSize = totalInSize;
        }

        /// <summary>
        /// Sets progress.
        /// </summary>
        /// <param name="inSize">Input size.</param>
        /// <param name="outSize">Output size.</param>
        public void SetProgress(long inSize, long outSize)
        {
            _progress.Value = (float) (inSize / (double) _totalInSize);
        }
    }
}