using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class AssetReference_Tests
    {
        private const string TEST_PREFAB_PATH = "Assets/Editor/Test/TestAsset.prefab";
        private const string TEST_PREFAB_UPDATE_PATH = "Assets/Editor/Test/TestAsset2.prefab";

        private readonly AssetInfo _info = new AssetInfo
        {
            Guid = "guid",
            Crc = "crc",
            Tags = new []{"test"},
            Uri = TEST_PREFAB_PATH,
            Version = 10
        };

        private readonly AssetInfo _infoUpdate = new AssetInfo
        {
            Guid = "guid",
            Crc = "crc",
            Tags = new[] { "test" },
            Uri = TEST_PREFAB_UPDATE_PATH,
            Version = 11
        };

        private AssetReference _reference;
        private GameObject _testAsset;
        private GameObject _testAssetUpdate;

        [SetUp]
        public void Setup()
        {
            _reference = new AssetReference(new DummyAssetLoader(), _info);
            _testAsset = AssetDatabase.LoadAssetAtPath<GameObject>(TEST_PREFAB_PATH);
            _testAssetUpdate = AssetDatabase.LoadAssetAtPath<GameObject>(TEST_PREFAB_UPDATE_PATH);

            if (null == _testAsset)
            {
                throw new Exception("Could not find test asset.");
            }

            if (null == _testAssetUpdate)
            {
                throw new Exception("Could not find test asset.");
            }
        }

        [Test]
        public void LoadAsset()
        {
            var successCalled = false;
            var failureCalled = false;

            _reference
                .Load<GameObject>()
                .OnSuccess(asset =>
                {
                    successCalled = true;

                    Assert.AreEqual(
                        _testAsset.GetInstanceID(),
                        asset.GetInstanceID());
                })
                .OnFailure(_ => failureCalled = true);

            Assert.IsTrue(successCalled);
            Assert.IsFalse(failureCalled);
        }

        [Test]
        public void AssetNullWithoutLoad()
        {
            Assert.IsNull(_reference.Asset<GameObject>());
        }

        [Test]
        public void AssetGetAsset()
        {
            var successCalled = false;
            var failureCalled = false;

            _reference
                .Load<GameObject>()
                .OnSuccess(asset =>
                {
                    Assert.AreEqual(
                        _testAsset.GetInstanceID(),
                        _reference.Asset<GameObject>().GetInstanceID());

                    successCalled = true;
                })
                .OnFailure(_ => failureCalled = true);

            Assert.IsTrue(successCalled);
            Assert.IsFalse(failureCalled);
        }

        [Test]
        public void LoadComponent()
        {
            var successCalled = false;
            var failureCalled = false;

            _reference
                .Load<TextMesh>()
                .OnSuccess(text =>
                {
                    Assert.NotNull(text);

                    successCalled = true;
                })
                .OnFailure(_ => failureCalled = true);

            Assert.IsTrue(successCalled);
            Assert.IsFalse(failureCalled);
        }

        [Test]
        public void UpdateAssetRef()
        {
            var successCalled = true;

            _reference.Update(_infoUpdate);

            _reference
                .Load<GameObject>()
                .OnSuccess(asset =>
                {
                    successCalled = true;

                    Assert.AreEqual(
                        _testAssetUpdate.GetInstanceID(),
                        asset.GetInstanceID());
                });

            Assert.IsTrue(successCalled);
        }

        [Test]
        public void UpdateChangeGuidFail()
        {
            Assert.Throws<ArgumentException>(delegate {
                _reference.Update(new AssetInfo
                {
                    Guid = "Different Guid"
                });
            });
        }

        [Test]
        public void WatchAssetRef()
        {
            var watchCalled = false;

            _reference.Watch((unwatch, reference) =>
            {
                watchCalled = true;

                Assert.AreSame(_reference, reference);
            });

            _reference.Update(_infoUpdate);

            Assert.IsTrue(watchCalled);
        }

        [Test]
        public void WatchIgnoreSameUpdate()
        {
            var watchCalled = false;

            _reference.Watch((unwatch, reference) =>
            {
                watchCalled = true;
            });

            _reference.Update(_info);

            Assert.IsFalse(watchCalled);
        }

        [Test]
        public void WatchTwice()
        {
            var watchCalled = 0;

            _reference.Watch((unwatch, reference) =>
            {
                watchCalled++;
            });

            _reference.Update(_infoUpdate);
            _reference.Update(_info);

            Assert.AreEqual(2, watchCalled);
        }

        [Test]
        public void WatchUnwatch()
        {
            var watchCalled = 0;

            _reference.Watch((unwatch, reference) =>
            {
                watchCalled++;
            });

            _reference.Update(_infoUpdate);

            Assert.AreEqual(2, watchCalled);
        }
    }
}