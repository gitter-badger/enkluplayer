using System.IO;
using System.Text;
using CreateAR.EnkluPlayer.Util;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Util
{
    [TestFixture]
    public class VersionedFileWriter_Tests
    {
        private VersionedFileWriter _writer;

        [Test]
        public void Setup()
        {
            const int maxVersions = 10;
            const int numWrites = 20;
            var folder = Path.Combine(
                Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    "Test"),
                "VersionedFileWriter");

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
            Directory.CreateDirectory(folder);

            _writer = new VersionedFileWriter(
                folder,
                "Test",
                "txt",
                maxVersions);

            for (var i = 0; i < numWrites; i++)
            {
                _writer.Write(Encoding.UTF8.GetBytes("This is test " + i + "."));
            }

            var files = Directory.GetFiles(folder);
            Assert.AreEqual(maxVersions, files.Length);
        }
    }
}