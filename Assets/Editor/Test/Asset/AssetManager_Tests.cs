using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Assets
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
                Queries = new StandardQueryResolver()
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
    }
}