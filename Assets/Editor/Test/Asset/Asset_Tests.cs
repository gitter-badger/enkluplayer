using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test.Assets
{
    [TestFixture]
    public class Asset_Tests
    {
        public const string TEST_PREFAB_PATH = "Assets/Editor/Test/TestAsset.prefab";
        public const string TEST_PREFAB_UPDATE_PATH = "Assets/Editor/Test/TestAsset2.prefab";

        public static readonly AssetData Data = new AssetData
        {
            Guid = "guid",
            Crc = "crc",
            Tags = "test",
            Uri = TEST_PREFAB_PATH,
            Version = 10
        };

        public static readonly AssetData DataUpdate = new AssetData
        {
            Guid = "guid",
            Crc = "crc",
            Tags = "test",
            Uri = TEST_PREFAB_UPDATE_PATH,
            Version = 11
        };

        private Asset _reference;
        private GameObject _testAsset;
        private GameObject _testAssetUpdate;

        [SetUp]
        public void Setup()
        {
            _reference = new Asset(new DummyAssetLoader(), Data);
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
            Assert.IsNull(_reference.As<GameObject>());
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
                        _reference.As<GameObject>().GetInstanceID());

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
        public void UpdateLeaveLoadedAsset()
        {
            _reference.Load<GameObject>();
            _reference.Update(DataUpdate);

            // check that we still have the old asset
            Assert.AreEqual(
                _testAsset.GetInstanceID(),
                _reference.As<GameObject>().GetInstanceID());
        }
        
        [Test]
        public void UpdateAssetRef()
        {
            var successCalled = true;

            // load existing asset first
            _reference.Load<GameObject>();

            // update to invalidate loaded asset
            _reference.Update(DataUpdate);

            // reload
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
                _reference.Update(new AssetData
                {
                    Guid = "Different Guid"
                });
            });
        }

        [Test]
        public void WatchRef()
        {
            var watchCalled = false;

            _reference.WatchData((unwatch, reference) =>
            {
                watchCalled = true;

                Assert.AreSame(_reference, reference);
            });

            _reference.Update(DataUpdate);

            Assert.IsTrue(watchCalled);
        }

        [Test]
        public void WatchRefIgnoreSameUpdate()
        {
            var watchCalled = false;

            _reference.WatchData((unwatch, reference) =>
            {
                watchCalled = true;
            });

            _reference.Update(Data);

            Assert.IsFalse(watchCalled);
        }
        
        [Test]
        public void WatchRefUnwatch()
        {
            var watchCalled = 0;

            _reference.WatchData((unwatch, reference) =>
            {
                watchCalled++;

                unwatch();
            });

            _reference.Update(DataUpdate);
            _reference.Update(DataUpdate);

            Assert.AreEqual(1, watchCalled);
        }

        [Test]
        public void WatchRefReturnedUnwatch()
        {
            var watchCalled = false;

            _reference.WatchData(reference =>
            {
                watchCalled = true;

                Assert.AreSame(_reference, reference);
            });

            _reference.Update(DataUpdate);

            Assert.IsTrue(watchCalled);
        }

        [Test]
        public void WatchRefUnwatchReturnedUnwatch()
        {
            var watchCalled = 0;

            var unwatch = _reference.WatchData(reference =>
            {
                watchCalled++;

                Assert.AreSame(_reference, reference);
            });

            _reference.Update(DataUpdate);

            unwatch();

            _reference.Update(DataUpdate);

            Assert.AreEqual(1, watchCalled);
        }

        [Test]
        public void WatchAsset()
        {
            var watchCalled = false;

            _reference.Watch<GameObject>((unwatch, asset) =>
            {
                watchCalled = true;

                Assert.AreEqual(
                    _testAsset.GetInstanceID(),
                    asset.GetInstanceID());
            });

            _reference.Load<GameObject>();

            Assert.IsTrue(watchCalled);
        }

        [Test]
        public void WatchAssetUnwatch()
        {
            var watchCalled = 0;

            _reference.Watch<GameObject>((unwatch, asset) =>
            {
                watchCalled++;

                unwatch();
            });

            _reference.Load<GameObject>();
            _reference.Update(DataUpdate);
            _reference.Load<GameObject>();

            Assert.AreEqual(1, watchCalled);
        }

        [Test]
        public void WatchAssetReturnedUnwatch()
        {
            var watchCalled = false;

            _reference.Watch<GameObject>(asset =>
            {
                watchCalled = true;

                Assert.AreEqual(
                    _testAsset.GetInstanceID(),
                    asset.GetInstanceID());
            });

            _reference.Load<GameObject>();

            Assert.IsTrue(watchCalled);
        }

        [Test]
        public void WatchAssetUnwatchReturnedUnwatch()
        {
            var watchCalled = 0;

            var unwatch = _reference.Watch<GameObject>(asset =>
            {
                watchCalled++;
            });
            
            _reference.Load<GameObject>();

            unwatch();

            _reference.Update(DataUpdate);
            _reference.Load<GameObject>();

            Assert.AreEqual(1, watchCalled);
        }

        [Test]
        public void AutoReload()
        {
            var watches = 0;

            _reference.Load<GameObject>();

            _reference.Watch<GameObject>(asset =>
            {
                watches++;
            });

            Assert.AreEqual(0, watches);

            _reference.AutoReload = true;

            Assert.AreEqual(0, watches);

            _reference.Update(DataUpdate);

            Assert.AreEqual(1, watches);
        }
    }
}