using System;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.Util;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Util
{
    [TestFixture]
    public class Lzma_IntegrationTests
    {
        private LzmaCompressor _compressor;
        private LzmaDecompressor _decompressor;

        [SetUp]
        public void Setup()
        {
            _compressor = new LzmaCompressor();
            _decompressor = new LzmaDecompressor();
        }

        [Test]
        public void EncodeDecode()
        {
            var rand = new Random(2614159);
            var original = new byte[1000000];
            rand.NextBytes(original);

            LoadProgress progress;
            var writerResults = _compressor.Compress(
                original,
                0,
                original.Length,
                out progress);

            var compressed = writerResults.Bytes;
            var readerResults = _decompressor.Decompress(
                compressed,
                out progress);

            var final = readerResults.Bytes;

            // compare!
            Assert.AreEqual(original.Length, final.Length);
            for (var i = 0; i < original.Length; i++)
            {
                if (original[i] != final[i])
                {
                    Assert.Fail("Mismatch at index " + i + ".");
                }
            }
        }
    }
}