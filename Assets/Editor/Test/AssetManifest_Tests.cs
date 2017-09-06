using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class AssetManifest_Tests
    {
        private readonly AssetInfo[] _infos = new[]
        {
            new AssetInfo
            {
                Guid = "a",
                Tags = new[] { "a", "b", "c" }
            },
            new AssetInfo
            {
                Guid = "b",
                Tags = new[]{ "a", "b" }
            },
            new AssetInfo
            {
                Guid = "c",
                Tags = new[]{ "c" }
            },
            new AssetInfo
            {
                Guid = "d",
                Tags = new[]{ "d" }
            }
        };
        private AssetManifest _manifest;

        [SetUp]
        public void Setup()
        {
            _manifest = new AssetManifest(new TagResolver());
            _manifest.Add(_infos);
        }

        [Test]
        public void Info()
        {
            Assert.AreSame("a", _manifest.Info("a").Guid);
            Assert.IsNull(_manifest.Info("e"));
        }

        [Test]
        public void Get()
        {
            Assert.AreSame("a", _manifest.Reference("a").Info.Guid);

            // case sensitive
            Assert.IsNull(_manifest.Reference("A"));
        }

        [Test]
        public void FindOne()
        {
            Assert.AreSame("a", _manifest.FindOne("a").Info.Guid);

            // case insensitive
            Assert.AreSame("a", _manifest.FindOne("A").Info.Guid);
        }

        [Test]
        public void Find()
        {
            Assert.AreSame("d", _manifest.Find("d")[0].Info.Guid);
            
            Assert.AreEqual(2, _manifest.Find("a,b").Length);
            Assert.AreEqual(1, _manifest.Find("a,c").Length);
            Assert.AreEqual(2, _manifest.Find("!a").Length);
            Assert.AreEqual(3, _manifest.Find("a c").Length);
        }
    }
}