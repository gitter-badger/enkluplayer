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
            var info = new AssetInfo
            {
                Guid = "This is a test"
            };

            _assets.Manifest.OnAssetAdded += reference =>
            {
                addedCalled = true;

                Assert.AreSame(info, reference.Info);
            };

            _service.Added(info);

            Assert.IsTrue(addedCalled);

            var updatedCalled = false;
            info = new AssetInfo
            {
                Guid = info.Guid
            };
            _assets.Manifest.OnAssetUpdated += reference =>
            {
                updatedCalled = true;

                Assert.AreSame(info, reference.Info);
            };

            _service.Updated(info);

            Assert.IsTrue(updatedCalled);
        }

        [Test]
        public void ServiceUpdateAssetRef()
        {
            var watchCalled = false;
            var info = AssetReference_Tests.Info;
            var infoUpdate = AssetReference_Tests.InfoUpdate;

            _assets.Manifest.Add(info);

            var reference = _assets.Manifest.Reference(info.Guid);

            Assert.IsNotNull(reference);
            Assert.AreSame(info, reference.Info);

            reference.Watch(_ =>
            {
                watchCalled = true;

                Assert.AreSame(infoUpdate, reference.Info);
            });

            _service.Updated(infoUpdate);

            Assert.IsTrue(watchCalled);
        }

        [Test]
        public void ServiceUpdateAsset()
        {
            var watchCalled = false;
            var info = AssetReference_Tests.Info;
            var infoUpdate = AssetReference_Tests.InfoUpdate;

            _assets.Manifest.Add(info);

            var reference = _assets.Manifest.Reference(info.Guid);

            Assert.IsNull(reference.Asset<GameObject>());

            reference.AutoReload = true;

            var loaded = reference.Asset<GameObject>();
            Assert.IsNotNull(loaded);

            reference.WatchAsset<GameObject>(asset =>
            {
                watchCalled = true;

                Assert.AreNotSame(loaded, asset);
            });

            _service.Updated(infoUpdate);

            Assert.IsTrue(watchCalled);
        }
    }
}