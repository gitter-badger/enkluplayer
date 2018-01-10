using System;

namespace CreateAR.SpirePlayer.Util
{
    /// <summary>
    /// Wraps LZMA encoder/decoder result.
    /// </summary>
    public class LzmaResult
    {
        /// <summary>
        /// Result bytes.
        /// </summary>
        public readonly byte[] Bytes;

        /// <summary>
        /// True iff successful.
        /// </summary>
        public readonly bool Success;

        /// <summary>
        /// Exception.
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="bytes">Resulting bytes.</param>
        internal LzmaResult(byte[] bytes)
        {
            Bytes = bytes;
            Success = true;
        }

        /// <summary>
        /// Creates a failure result.
        /// </summary>
        /// <param name="exception">Exception.</param>
        internal LzmaResult(Exception exception)
        {
            Exception = exception;
            Success = false;
        }
    }
}