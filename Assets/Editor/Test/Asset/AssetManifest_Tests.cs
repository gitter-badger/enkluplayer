using System;
using CreateAR.EnkluPlayer.Assets;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Assets
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
            Assert.AreSame("a", _manifest.Asset("a", -1).Data.Guid);

            // case sensitive
            Assert.IsNull(_manifest.Asset("A", -1));
        }

        [Test]
        public void FindOne()
        {
            Assert.AreSame("a", _manifest.FindOne("a", -1).Data.Guid);

            // case insensitive
            Assert.AreSame("a", _manifest.FindOne("A", -1).Data.Guid);
        }

        [Test]
        public void Find()
        {
            Assert.AreSame("d", _manifest.Find("d", -1)[0].Data.Guid);
            
            Assert.AreEqual(2, _manifest.Find("a,b", -1).Length);
            Assert.AreEqual(1, _manifest.Find("a,c", -1).Length);
            Assert.AreEqual(2, _manifest.Find("!a", -1).Length);
            Assert.AreEqual(3, _manifest.Find("a c", -1).Length);
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

                Assert.AreSame(asset, added);
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

            _manifest.OnAssetUpdated += data =>
            {
                eventCalled = true;

                Assert.AreSame(info, data);
            };

            _manifest.Update(info);

            Assert.IsTrue(eventCalled);
        }

        [Test]
        public void WatchForUpdate()
        {
            var eventCalled = false;
            var info = new AssetData
            {
                Guid = "a"
            };

            _manifest.WatchUpdate("a", data =>
            {
                eventCalled = true;
            });

            _manifest.Update(info);

            Assert.IsTrue(eventCalled);
        }

        [Test]
        public void UnwatchForUpdate()
        {
            var eventCalled = false;
            var info = new AssetData
            {
                Guid = "a"
            };

            var unwatch = _manifest.WatchUpdate("a", data =>
            {
                eventCalled = true;
            });
            unwatch();

            _manifest.Update(info);

            Assert.IsFalse(eventCalled);
        }

        [Test]
        public void WatchGoodUpdate()
        {
            var eventCalled = false;
            var info = new AssetData
            {
                Guid = "b"
            };

            _manifest.WatchUpdate("a", data =>
            {
                eventCalled = true;
            });

            _manifest.Update(info);

            Assert.IsFalse(eventCalled);
        }

        [Test]
        public void WatchRemove()
        {
            var eventCalled = false;
            
            _manifest.WatchRemove("a", () =>
            {
                eventCalled = true;
            });

            _manifest.Remove("a");

            Assert.IsTrue(eventCalled);
        }

        [Test]
        public void UnwatchRemove()
        {
            var eventCalled = false;

            var unwatch = _manifest.WatchRemove("a", () =>
            {
                eventCalled = true;
            });
            unwatch();

            _manifest.Remove("a");

            Assert.IsFalse(eventCalled);
        }
    }
}