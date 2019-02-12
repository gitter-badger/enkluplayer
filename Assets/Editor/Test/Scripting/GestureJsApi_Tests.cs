using System;
using Assets.Source.Player.Scripting;
using CreateAR.EnkluPlayer.Scripting;
using Jint;
using Jint.Native;
using NUnit.Framework;
using UnityEngine;

// TODO: Make these tests more comprehensive!

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class GestureJsApi_Tests
    {
        private Engine _engine;
        private TestGestureManager _gestureManager;

        [SetUp]
        public void Setup()
        {
            _engine = ScriptingHostFactory.NewEngine(false);

            _gestureManager = new TestGestureManager();
            _gestureManager.SetPointers(new uint[1] {12345});

            _engine.SetValue("require", new Func<string, JsValue>(
                value => JsValue.FromObject(_engine, new GestureJsInterface(_gestureManager))
            ));
        }

        [Test]
        public void Pose()
        {
            var output = _engine.Run<Vec3>(
                "var gestures = require('gestures');" +
                "var pose = gestures.pose(12345);" +

                "pose.origin;"
            );

            Assert.IsTrue(Mathf.Approximately(output.x, 1));
            Assert.IsTrue(Mathf.Approximately(output.y, 1));
            Assert.IsTrue(Mathf.Approximately(output.z, 1));
        }

        private class TestGestureManager : IGestureManager
        {
            public event Action<uint> OnPointerStarted;
            public event Action<uint> OnPointerEnded;
            public event Action<uint> OnPointerPressed;
            public event Action<uint> OnPointerReleased;
            public uint[] Pointers { get; private set; }

            public void SetPointers(uint[] ids)
            {
                Pointers = ids;
            }

            public void Initialize()
            {
                throw new NotImplementedException();
            }

            public void Uninitialize()
            {
                throw new NotImplementedException();
            }

            public bool TryGetPointerOrigin(uint id, out Vector3 position)
            {
                if (id == 12345)
                {
                    position = Vector3.one;
                    return true;
                }
                throw new ArgumentException();
            }

            public bool TryGetPointerForward(uint id, out Vector3 forward)
            {
                throw new NotImplementedException();
            }

            public bool TryGetPointerUp(uint id, out Vector3 up)
            {
                throw new NotImplementedException();
            }

            public bool TryGetPointerRight(uint id, out Vector3 right)
            {
                throw new NotImplementedException();
            }

            public bool TryGetPointerRotation(uint id, out Quaternion rotation)
            {
                throw new NotImplementedException();
            }

            public bool TryGetPointerVelocity(uint id, out Vector3 velocity)
            {
                throw new NotImplementedException();
            }
        }
    }
}