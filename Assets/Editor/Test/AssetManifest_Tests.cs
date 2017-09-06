using System;
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
            _manifest = new AssetManifest(
                new TagResolver(),
                new DummyAssetLoader());
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

        [Test]
        public void AddEvent()
        {
            var called = false;
            var asset = new AssetInfo
            {
                Guid = "meh"
            };

            _manifest.OnNewAsset += added =>
            {
                called = true;

                Assert.AreSame(asset, added.Info);
            };
            _manifest.Add(asset);

            Assert.IsTrue(called);
        }

        [Test]
        public void AddSameGuidError()
        {
            Assert.Throws<ArgumentException>(
                delegate
                {
                    _manifest.Add(new AssetInfo
                    {
                        Guid = "a"
                    });
                });
        }

        [Test]
        public void UpdateNonExisting()
        {
            var eventCalled = false;
            _manifest.OnUpdatedAsset += reference => eventCalled = true;

            Assert.Throws<ArgumentException>(delegate
            {
                _manifest.Update(new AssetInfo
                {
                    Guid = "nonexistent"
                });
            });

            Assert.IsFalse(eventCalled);
        }

        [Test]
        public void UpdateBadInfo()
        {
            var eventCalled = false;
            _manifest.OnUpdatedAsset += reference => eventCalled = true;

            Assert.Throws<ArgumentException>(delegate
            {
                _manifest.Update(new AssetInfo());
            });

            Assert.IsFalse(eventCalled);
        }

        [Test]
        public void UpdateEvent()
        {
            var eventCalled = false;
            var info = new AssetInfo
            {
                Guid = "a"
            };

            _manifest.OnUpdatedAsset += reference =>
            {
                eventCalled = true;

                Assert.AreSame(info, reference.Info);
            };

            _manifest.Update(info);

            Assert.IsTrue(eventCalled);
        }
    }
}