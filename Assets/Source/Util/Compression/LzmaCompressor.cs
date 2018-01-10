using System;
using System.IO;
using CreateAR.SpirePlayer.Assets;
using SevenZip.Compression.LZMA;

namespace CreateAR.SpirePlayer.Util
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
        /// <param name="progress">Output progress.</param>
        /// <returns>Result object.</returns>
        public LzmaResult Compress(
            ref byte[] bytes,
            out LoadProgress progress)
        {
            progress = new LoadProgress();

            var inputLength = bytes.Length;
            var coderProgress = new LzmaProgressWrapper(progress, inputLength);
            using (var input = new MemoryStream(bytes))
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
                            inputLength,
                            -1,
                            coderProgress);
                    }
                    catch (Exception exception)
                    {
                        return new LzmaResult(exception);
                    }

                    return new LzmaResult(output.GetBuffer());
                }
            }
        }
    }
}