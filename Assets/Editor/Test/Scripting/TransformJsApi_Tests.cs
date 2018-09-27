using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using Jint;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class TransformJsApi_Tests
    {
        private Engine _engine;
        private ElementJs _element;
        private GameObject _gameObject;

        [SetUp]
        public void Setup()
        {
            _engine = new Engine();
            _element = new ElementJs(null, null, _engine, new Element());
            _engine.SetValue("this", _element);

            _gameObject = new GameObject("TransformJsApi Tests");
        }

        [Test]
        public void Forward()
        {
            // Test that our forward matches Unity's over a variety of euler rotations
            for (int i = 0; i < TestData.EulerArray.Length; i++)
            {
                var euler = TestData.EulerArray[i];
                _element.transform.rotation = Quat.Euler(euler);
                _gameObject.transform.rotation = Quaternion.Euler(euler.ToVector());

                // Why does this require this.this?!
                var enkluForward = _engine.Run<Vec3>("this.this.transform.forward");
                var unityForward = _gameObject.transform.forward;
                Assert.IsTrue(Vector3.Angle(enkluForward.ToVector(), unityForward) < Mathf.Epsilon);
            }
        }
    }
}