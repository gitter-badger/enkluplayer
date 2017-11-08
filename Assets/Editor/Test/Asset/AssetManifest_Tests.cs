using System;
using CreateAR.SpirePlayer.Assets;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Assets
{
    [TestFixture]
    public class AssetManifest_Tests
    {
        private readonly AssetData[] _assets = {
            new AssetData
            {
                Guid = "a",
                Tags = "a,b,c"
            },
            new AssetData
            {
                Guid = "b",
                Tags = "a,b"
            },
            new AssetData
            {
                Guid = "c",
                Tags = "c"
            },
            new AssetData
            {
                Guid = "d",
                Tags = "d"
            }
        };
        private AssetManifest _manifest;

        [SetUp]
        public void Setup()
        {
            _manifest = new AssetManifest(
                new StandardQueryResolver(),
                new DummyAssetLoader());
            _manifest.Add(_assets);
        }

        [Test]
        public void Info()
        {
            Assert.AreSame("a", _manifest.Data("a").Guid);
            Assert.IsNull(_manifest.Data("e"));
        }

        [Test]
        public void Get()
        {
            Assert.AreSame("a", _manifest.Asset("a").Data.Guid);

            // case sensitive
            Assert.IsNull(_manifest.Asset("A"));
        }

        [Test]
        public void FindOne()
        {
            Assert.AreSame("a", _manifest.FindOne("a").Data.Guid);

            // case insensitive
            Assert.AreSame("a", _manifest.FindOne("A").Data.Guid);
        }

        [Test]
        public void Find()
        {
            Assert.AreSame("d", _manifest.Find("d")[0].Data.Guid);
            
            Assert.AreEqual(2, _manifest.Find("a,b").Length);
            Assert.AreEqual(1, _manifest.Find("a,c").Length);
            Assert.AreEqual(2, _manifest.Find("!a").Length);
            Assert.AreEqual(3, _manifest.Find("a c").Length);
        }

        [Test]
        public void AddEvent()
        {
            var called = false;
            var asset = new AssetData
            {
                Guid = "meh"
            };

            _manifest.OnAssetAdded += added =>
            {
                called = true;

                Assert.AreSame(asset, added.Data);
            };
            _manifest.Add(asset);

            Assert.IsTrue(called);
        }

        [Test]
        public void RemoveEvent()
        {
            var called = 0;

            _manifest.OnAssetRemoved += removed =>
            {
                called++;
            };

            _manifest.Remove("a", "b");

            Assert.AreEqual(2, called);
        }

        [Test]
        public void RemoveAssetEvent()
        {
            var called = false;

            var reference = _manifest.Asset("a");
            reference.OnRemoved += r =>
            {
                called = true;

                Assert.AreSame(reference, r);
            };

            _manifest.Remove("a");

            Assert.IsTrue(called);
        }

        [Test]
        public void AddSameGuidError()
        {
            Assert.Throws<ArgumentException>(
                delegate
                {
                    _manifest.Add(new AssetData
                    {
                        Guid = "a"
                    });
                });
        }

        [Test]
        public void UpdateNonExisting()
        {
            var eventCalled = false;
            _manifest.OnAssetUpdated += reference => eventCalled = true;

            Assert.Throws<ArgumentException>(delegate
            {
                _manifest.Update(new AssetData
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
            _manifest.OnAssetUpdated += reference => eventCalled = true;

            Assert.Throws<ArgumentException>(delegate
            {
                _manifest.Update(new AssetData());
            });

            Assert.IsFalse(eventCalled);
        }

        [Test]
        public void UpdateEvent()
        {
            var eventCalled = false;
            var info = new AssetData
            {
                Guid = "a"
            };

            _manifest.OnAssetUpdated += reference =>
            {
                eventCalled = true;

                Assert.AreSame(info, reference.Data);
            };

            _manifest.Update(info);

            Assert.IsTrue(eventCalled);
        }
    }
}