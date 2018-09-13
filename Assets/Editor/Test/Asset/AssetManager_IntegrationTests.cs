using CreateAR.EnkluPlayer.Assets;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Assets
{
    [TestFixture]
    public class AssetManager_IntegrationTests
    {
        private AssetManagerConfiguration _configuration;
        private AssetManager _assets;

        private readonly AssetData[] _infos = {
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
            _configuration = new AssetManagerConfiguration
            {
                Loader = new DummyAssetLoader(),
                Queries = new StandardQueryResolver()
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

            _assets.Manifest.Add(info);

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

            _assets.Manifest.Update(info);

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

            _assets.Manifest.Update(infoUpdate);

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

            reference.Watch<GameObject>((error, asset) =>
            {
                watchCalled = true;

                Assert.AreNotSame(loaded, asset);
            });

            _assets.Manifest.Update(infoUpdate);

            Assert.IsTrue(watchCalled);
        }
    }
}