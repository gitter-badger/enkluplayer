using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class AssetReference_Tests
    {
        private const string TEST_PRAFAB_PATH = "Assets/Editor/Test/TestAsset.prefab";

        private readonly AssetInfo _info = new AssetInfo
        {
            Guid = "guid",
            Crc = "crc",
            Tags = new []{"test"},
            Uri = TEST_PRAFAB_PATH,
            Version = 10
        };

        private AssetReference _reference;
        private GameObject _testAsset;

        [SetUp]
        public void Setup()
        {
            _reference = new AssetReference(new DummyAssetLoader(), _info);
            _testAsset = AssetDatabase.LoadAssetAtPath<GameObject>(TEST_PRAFAB_PATH);

            if (null == _testAsset)
            {
                throw new Exception("Could not find test asset.");
            }
        }

        [Test]
        public void NullAssetWithoutLoad()
        {
            Assert.IsNull(_reference.Asset<GameObject>());
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
        public void LoadAssetGetAsset()
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
        
    }
}