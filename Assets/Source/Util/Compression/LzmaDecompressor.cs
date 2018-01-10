using System;
using System.IO;
using CreateAR.SpirePlayer.Assets;
using SevenZip.Compression.LZMA;

namespace CreateAR.SpirePlayer.Util
{
    /// <summary>
    /// Decodes LZMA compressed data.
    /// </summary>
    public class LzmaDecompressor
    {
        /// <summary>
        /// LZMA decoder.
        /// </summary>
        private readonly Decoder _decoder = new Decoder();

        /// <summary>
        /// Decompresses bytes via LZMA.
        /// </summary>
        /// <param name="bytes">Compressed bytes.</param>
        /// <param name="progress">Decompression progress.</param>
        /// <returns>Result object.</returns>
        public LzmaResult Decompress(
            ref byte[] bytes,
            out LoadProgress progress)
        {
            progress = new LoadProgress();

            using (var input = new MemoryStream(bytes))
            {
                using (var output = new MemoryStream())
                {
                    try
                    {
                        // decoder props
                        var properties = new byte[5];
                        input.Read(properties, 0, 5);

                        // decompress file size
                        var outputLenBytes = new byte[8];
                        input.Read(outputLenBytes, 0, 8);
                        var fileLen = BitConverter.ToInt64(outputLenBytes, 0);

                        var coderProgress = new LzmaProgressWrapper(progress, fileLen);

                        _decoder.SetDecoderProperties(properties);
                        _decoder.Code(
                            input,
                            output,
                            bytes.Length,
                            fileLen,
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