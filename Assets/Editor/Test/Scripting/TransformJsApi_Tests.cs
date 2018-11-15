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
        private ElementJs _parent;
        private ElementJs _element;
        private GameObject _gameObject;

        [SetUp]
        public void Setup()
        {
            var parent = new Widget(new GameObject("TransformJsApi_Tests"), null, null, null);
            var child = new Widget(new GameObject("TransformJsApi_Tests"), null, null, null);
            
            parent.Load(new ElementData(), parent.Schema, new Element[0]);
            child.Load(new ElementData(), child.Schema, new Element[0]);
            
            parent.AddChild(child);
            
            _engine = new Engine();
            _parent = new ElementJs(null, null, parent);
            _element = new ElementJs(null, null, child);
            _engine.SetValue("element", _element);
            _engine.SetValue("v", Vec3Methods.Instance);
            _engine.SetValue("q", QuatMethods.Instance);

            _gameObject = new GameObject("TransformJsApi Tests");
        }

        [Test]
        public void WorldPosition()
        {
            _parent.transform.position = new Vec3(1, 2, 3);
            _element.transform.position = new Vec3(-3, 2, -1);
            
            Assert.IsTrue(_element.transform.worldPosition.Approximately(new Vec3(-2, 4, 2)));
        }

        [Test]
        public void WorldRotation()
        {
            _parent.transform.rotation = Quat.Euler(45, 90, 12);
            _element.transform.rotation = Quat.Euler(-45, 30, 12);
            
            Assert.AreEqual(
                ((Widget) _element.Element).GameObject.transform.rotation.ToQuat(), 
                _element.transform.worldRotation);
        }

        [Test]
        public void WorldScale()
        {
            _parent.transform.scale = new Vec3(2, 4, 2);
            _element.transform.scale = new Vec3(2, -1, 0.5f);
            
            Assert.IsTrue(_element.transform.worldScale.Approximately(new Vec3(4, -4, 1)));
        }

        [Test]
        public void Forward()
        {
            // Test that our forward matches Unity's over a variety of euler rotations
            for (var i = 0; i < TestData.EulerArray.Length; i++)
            {
                var euler = TestData.EulerArray[i];
                _element.transform.rotation = Quat.Euler(euler);
                _gameObject.transform.rotation = Quaternion.Euler(euler.ToVector());
                
                var enkluForward = _engine.Run<Vec3>("element.transform.forward");
                var unityForward = _gameObject.transform.forward;
                Assert.IsTrue(Vector3.Angle(enkluForward.ToVector(), unityForward) < Mathf.Epsilon);
            }
        }

        [Test]
        public void LookAt()
        {
            var rotation = _engine.Run<Quat>(@"
                element.transform.lookAt(v.normalize(v.create(1, 0, 0)));
                element.transform.rotation;
            ");

            var euler = rotation.ToQuaternion().eulerAngles;
            Assert.IsTrue(euler.Approximately(new Vector3(0, 90, 0)));
        }

        [Test]
        public void TransformPoint()
        {
            _element.transform.position = new Vec3(1, 2, 3);
            _element.transform.rotation = Quat.Euler(0, 90, 0);
            _element.transform.scale = new Vec3(3, -1, 0.5f);
            
            var point = _engine.Run<Vec3>(@"
                element.transform.transformPoint(v.create(4, -5, 6));
            ");

            var widget = (Widget) _element.Element;

            var unity = widget.GameObject.transform.TransformPoint(4, -5, 6).ToVec();
            
            // TODO: Update Quat math to use doubles, for parity with Unity?
            Assert.IsTrue(unity.Approximately(point, 0.00001f));
        }
    }    
}
