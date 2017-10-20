using NUnit.Framework;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class AssetManager_IntegrationTests
    {
        private AssetManagerConfiguration _configuration;
        private DummyAssetUpdateService _service;
        private AssetManager _assets;

        private readonly AssetData[] _infos = new[]
        {
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

        [SetUp]
        public void Setup()
        {
            _service = new DummyAssetUpdateService();
            _configuration = new AssetManagerConfiguration
            {
                Loader = new DummyAssetLoader(),
                Queries = new StandardQueryResolver(),
                Service = _service
            };

            _assets = new AssetManager();
            _assets.Initialize(_configuration);
            _assets.Manifest.Add(_infos);
        }

        [Test]
        public void ServiceAddUpdate()
        {
            var addedCalled = false;
            var info = new AssetData
            {
                Guid = "This is a test"
            };

            _assets.Manifest.OnAssetAdded += reference =>
            {
                addedCalled = true;

                Assert.AreSame(info, reference.Data);
            };

            _service.Added(info);

            Assert.IsTrue(addedCalled);

            var updatedCalled = false;
            info = new AssetData
            {
                Guid = info.Guid
            };
            _assets.Manifest.OnAssetUpdated += reference =>
            {
                updatedCalled = true;

                Assert.AreSame(info, reference.Data);
            };

            _service.Updated(info);

            Assert.IsTrue(updatedCalled);
        }

        [Test]
        public void ServiceUpdateAssetRef()
        {
            var watchCalled = false;
            var info = Asset_Tests.Data;
            var infoUpdate = Asset_Tests.DataUpdate;

            _assets.Manifest.Add(info);

            var reference = _assets.Manifest.Asset(info.Guid);

            Assert.IsNotNull(reference);
            Assert.AreSame(info, reference.Data);

            reference.WatchData(_ =>
            {
                watchCalled = true;

                Assert.AreSame(infoUpdate, reference.Data);
            });

            _service.Updated(infoUpdate);

            Assert.IsTrue(watchCalled);
        }

        [Test]
        public void ServiceUpdateAsset()
        {
            var watchCalled = false;
            var info = Asset_Tests.Data;
            var infoUpdate = Asset_Tests.DataUpdate;

            _assets.Manifest.Add(info);

            var reference = _assets.Manifest.Asset(info.Guid);

            Assert.IsNull(reference.As<GameObject>());

            reference.AutoReload = true;

            var loaded = reference.As<GameObject>();
            Assert.IsNotNull(loaded);

            reference.Watch<GameObject>(asset =>
            {
                watchCalled = true;

                Assert.AreNotSame(loaded, asset);
            });

            _service.Updated(infoUpdate);

            Assert.IsTrue(watchCalled);
        }
    }
}