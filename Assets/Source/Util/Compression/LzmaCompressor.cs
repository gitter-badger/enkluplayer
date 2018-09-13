using System;
using System.IO;
using CreateAR.EnkluPlayer.Assets;
using SevenZip.Compression.LZMA;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Object that compresses via LZMA.
    /// </summary>
    public class LzmaCompressor
    {
        /// <summary>
        /// LZMA encoder.
        /// </summary>
        private readonly Encoder _encoder = new Encoder();

        /// <summary>
        /// Compresses bytes via LZMA.
        /// </summary>
        /// <param name="bytes">Reference to input bytes.</param>
        /// <param name="len">Byte length.</param>
        /// <param name="progress">Output progress.</param>
        /// <param name="offset">Offset into bytes.</param>
        /// <returns>Result object.</returns>
        public LzmaResult Compress(
            byte[] bytes,
            int offset,
            int len,
            out LoadProgress progress)
        {
            progress = new LoadProgress();

            var coderProgress = new LzmaProgressWrapper(progress, len);
            using (var input = new MemoryStream(bytes, offset, len))
            {
                using (var output = new MemoryStream())
                {
                    try
                    {
                        _encoder.WriteCoderProperties(output);
                        output.Write(BitConverter.GetBytes((long)bytes.Length), 0, 8);
                        _encoder.Code(
                            input,
                            output,
                            len,
                            -1,
                            coderProgress);
                    }
                    catch (Exception exception)
                    {
                        return new LzmaResult(exception);
                    }

                    return new LzmaResult(output.ToArray());
                }
            }
        }
    }
}