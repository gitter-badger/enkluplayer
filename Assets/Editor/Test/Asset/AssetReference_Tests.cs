using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class AssetReference_Tests
    {
        public const string TEST_PREFAB_PATH = "Assets/Editor/Test/TestAsset.prefab";
        public const string TEST_PREFAB_UPDATE_PATH = "Assets/Editor/Test/TestAsset2.prefab";

        public static readonly AssetInfo Info = new AssetInfo
        {
            Guid = "guid",
            Crc = "crc",
            Tags = new []{"test"},
            Uri = TEST_PREFAB_PATH,
            Version = 10
        };

        public static readonly AssetInfo InfoUpdate = new AssetInfo
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
            _reference = new AssetReference(new DummyAssetLoader(), Info);
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
        public void UpdateLeaveLoadedAsset()
        {
            _reference.Load<GameObject>();
            _reference.Update(InfoUpdate);

            // check that we still have the old asset
            Assert.AreEqual(
                _testAsset.GetInstanceID(),
                _reference.Asset<GameObject>().GetInstanceID());
        }
        
        [Test]
        public void UpdateAssetRef()
        {
            var successCalled = true;

            // load existing asset first
            _reference.Load<GameObject>();

            // update to invalidate loaded asset
            _reference.Update(InfoUpdate);

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
                _reference.Update(new AssetInfo
                {
                    Guid = "Different Guid"
                });
            });
        }

        [Test]
        public void WatchRef()
        {
            var watchCalled = false;

            _reference.Watch((unwatch, reference) =>
            {
                watchCalled = true;

                Assert.AreSame(_reference, reference);
            });

            _reference.Update(InfoUpdate);

            Assert.IsTrue(watchCalled);
        }

        [Test]
        public void WatchRefIgnoreSameUpdate()
        {
            var watchCalled = false;

            _reference.Watch((unwatch, reference) =>
            {
                watchCalled = true;
            });

            _reference.Update(Info);

            Assert.IsFalse(watchCalled);
        }
        
        [Test]
        public void WatchRefUnwatch()
        {
            var watchCalled = 0;

            _reference.Watch((unwatch, reference) =>
            {
                watchCalled++;

                unwatch();
            });

            _reference.Update(InfoUpdate);
            _reference.Update(InfoUpdate);

            Assert.AreEqual(1, watchCalled);
        }

        [Test]
        public void WatchRefReturnedUnwatch()
        {
            var watchCalled = false;

            var unwatch = _reference.Watch(reference =>
            {
                watchCalled = true;

                Assert.AreSame(_reference, reference);
            });

            _reference.Update(InfoUpdate);

            Assert.IsTrue(watchCalled);
        }

        [Test]
        public void WatchRefUnwatchReturnedUnwatch()
        {
            var watchCalled = 0;

            var unwatch = _reference.Watch(reference =>
            {
                watchCalled++;

                Assert.AreSame(_reference, reference);
            });

            _reference.Update(InfoUpdate);

            unwatch();

            _reference.Update(InfoUpdate);

            Assert.AreEqual(1, watchCalled);
        }

        [Test]
        public void WatchAsset()
        {
            var watchCalled = false;

            _reference.WatchAsset<GameObject>((unwatch, asset) =>
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

            _reference.WatchAsset<GameObject>((unwatch, asset) =>
            {
                watchCalled++;

                unwatch();
            });

            _reference.Load<GameObject>();
            _reference.Update(InfoUpdate);
            _reference.Load<GameObject>();

            Assert.AreEqual(1, watchCalled);
        }

        [Test]
        public void WatchAssetReturnedUnwatch()
        {
            var watchCalled = false;

            var unwatch = _reference.WatchAsset<GameObject>(asset =>
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

            var unwatch = _reference.WatchAsset<GameObject>(asset =>
            {
                watchCalled++;
            });
            
            _reference.Load<GameObject>();

            unwatch();

            _reference.Update(InfoUpdate);
            _reference.Load<GameObject>();

            Assert.AreEqual(1, watchCalled);
        }

        [Test]
        public void AutoReload()
        {
            var watches = 0;

            _reference.Load<GameObject>();

            _reference.WatchAsset<GameObject>(asset =>
            {
                watches++;
            });

            Assert.AreEqual(0, watches);

            _reference.AutoReload = true;

            Assert.AreEqual(0, watches);

            _reference.Update(InfoUpdate);

            Assert.AreEqual(1, watches);
        }
    }
}