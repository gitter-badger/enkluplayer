using System;
using CreateAR.EnkluPlayer.Assets;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Assets
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
        private Asset _errorReference;
        private GameObject _testAsset;
        private GameObject _testAssetUpdate;

        [SetUp]
        public void Setup()
        {
            _reference = new Asset(new DummyAssetLoader(), Data, 0);
            _errorReference = new Asset(new DummyAssetLoader("Error!"), Data, 0);

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
        public void LoadError()
        {
            _errorReference.Load<GameObject>();

            Assert.IsFalse(string.IsNullOrEmpty(_errorReference.Error));
        }

        [Test]
        public void ConfigurationAdd()
        {
            var called = false;

            _reference.OnConfigurationUpdated += flags =>
            {
                called = true;

                Assert.IsTrue(0 != (flags & AssetFlags.Hidden));
            };

            _reference.AddConfiguration(AssetFlags.Hidden);

            Assert.IsTrue(called);
        }

        [Test]
        public void ConfigurationRemove()
        {
            var called = false;

            _reference.AddConfiguration(AssetFlags.Hidden);

            _reference.OnConfigurationUpdated += flags =>
            {
                called = true;

                Assert.IsTrue(0 == (flags & AssetFlags.Hidden));
            };

            _reference.RemoveConfiguration(AssetFlags.Hidden);

            Assert.IsTrue(called);
        }
    }
}