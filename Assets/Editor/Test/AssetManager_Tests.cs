using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class AssetManager_Tests
    {
        private AssetManagerConfiguration _configuration;
        private AssetManager _assets;

        [SetUp]
        public void Setup()
        {
            _configuration = new AssetManagerConfiguration
            {
                Loader = new DummyAssetLoader(),
                Queries = new TagResolver()
            };

            _assets = new AssetManager();
        }
        
        [Test]
        public void InitializeNoLoader()
        {
            var failure = false;

            _assets
                .Initialize(new AssetManagerConfiguration())
                .OnFailure(_ => failure = true);

            Assert.IsTrue(failure);
        }

        [Test]
        public void InitializeNoQueries()
        {
            var failure = false;

            _assets
                .Initialize(new AssetManagerConfiguration
                {
                    Loader = new DummyAssetLoader()
                })
                .OnFailure(_ => failure = true);

            Assert.IsTrue(failure);
        }

        [Test]
        public void Initialize()
        {
            var success = false;

            _assets
                .Initialize(_configuration)
                .OnSuccess(_ => success = true);

            Assert.IsTrue(success);
        }

        [Test]
        public void InitializeTwiceFail()
        {
            var failure = false;

            _assets.Initialize(_configuration);
            _assets
                .Initialize(_configuration)
                .OnFailure(_ => failure = true);

            Assert.IsTrue(failure);
        }

        [Test]
        public void InitializeUninitialize()
        {
            var success = false;

            _assets.Initialize(_configuration);
            _assets.Uninitialize();
            _assets
                .Initialize(_configuration)
                .OnSuccess(_ => success = true);

            Assert.IsTrue(success);
        }

        [Test]
        public void ServiceAddUpdate()
        {
            var service = new DummyAssetUpdateService();
            _configuration.Service = service;
            _assets.Initialize(_configuration);

            var addedCalled = false;
            var info = new AssetInfo
            {
                Guid = "This is a test"
            };
            _assets.Manifest.OnNewAsset += reference =>
            {
                addedCalled = true;

                Assert.AreSame(info, reference.Info);
            };

            service.Added(info);

            Assert.IsTrue(addedCalled);
        
            var updatedCalled = false;
            info = new AssetInfo
            {
                Guid = info.Guid
            };
            _assets.Manifest.OnUpdatedAsset += reference =>
            {
                updatedCalled = true;

                Assert.AreSame(info, reference.Info);
            };

            service.Updated(info);

            Assert.IsTrue(updatedCalled);
        }
    }
}