using CreateAR.Commons.Unity.Logging;
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
        // Use a non-epsilon tolerance, since Quat math uses floats and Unity's uses doubles.
        // TODO: Update Quat math to use doubles, for parity with Unity?
        private const float TOLERANCE = 0.0001f;
        
        private Engine _engine;
        private ElementJs _parent;
        private ElementJs _element;
        private GameObject _unityParent;
        private GameObject _unityChild;

        [SetUp]
        public void Setup()
        {
            var parent = new Widget(new GameObject("TransformJsApi_Tests"), null, null, null);
            var child = new Widget(new GameObject("TransformJsApi_Tests"), null, null, null);
            
            parent.Load(new ElementData(), parent.Schema, new Element[0]);
            child.Load(new ElementData(), child.Schema, new Element[0]);
            parent.AddChild(child);
            
            var cache = new ElementJsCache(new ElementJsFactory(null));
            _engine = new Engine();
            _parent = new ElementJs(null, cache, parent);
            _element = new ElementJs(null, cache, child);
            _engine.SetValue("element", _element);
            _engine.SetValue("v", Vec3Methods.Instance);
            _engine.SetValue("q", QuatMethods.Instance);

            _unityParent = new GameObject("TransformJsApi Tests");
            _unityChild = new GameObject("TransformJsApi Tests");
            _unityChild.transform.SetParent(_unityParent.transform);

            var parentPos = new Vec3(1, 2, 3);
            var parentRot = Quat.Euler(45, 90, 12);
            var parentScale = new Vec3(2, 4, 2);

            _unityParent.transform.rotation = parentRot.ToQuaternion();
            _parent.transform.rotation = parentRot;

            _unityParent.transform.position = parentPos.ToVector();
            _parent.transform.position = parentPos;

            _unityParent.transform.localScale = parentScale.ToVector();
            _parent.transform.scale = parentScale;
        }

        [Test]
        public void WorldPosition()
        {
            // Get
            var childPos = new Vec3(-3, 2, -1);
            _element.transform.position = childPos;
            _unityChild.transform.localPosition = childPos.ToVector();
            
            Assert.IsTrue(CompareVectors(_unityChild.transform.position, _element.transform.worldPosition));
            Assert.IsTrue(CompareVectors(_unityChild.transform.localPosition, _element.transform.position));
        }

        [Test]
        public void WorldRotation()
        {
            // Get
            var childRot = Quat.Euler(-45, 30, 12);
            _element.transform.rotation = childRot;
            _unityChild.transform.localRotation = childRot.ToQuaternion();
            
            Assert.IsTrue(CompareVectors(
                _unityChild.transform.rotation.eulerAngles, 
                _element.transform.worldRotation.ToQuaternion().eulerAngles.ToVec()));
            Assert.IsTrue(CompareVectors(
                _unityChild.transform.localRotation.eulerAngles,
                _element.transform.rotation.ToQuaternion().eulerAngles.ToVec()));
        }

        [Test]
        public void WorldScale()
        {
            // Get
            var childScale = new Vec3(2, -1, 0.5f);
            _element.transform.scale = childScale;
            _unityChild.transform.localScale = childScale.ToVector();
            
            Assert.IsTrue(CompareVectors(_unityChild.transform.lossyScale, _element.transform.worldScale));
            Assert.IsTrue(CompareVectors(_unityChild.transform.localScale, _element.transform.scale));
        }

        [Test]
        public void Forward()
        {
            // Test that our forward matches Unity's over a variety of euler rotations
            for (var i = 0; i < TestData.EulerArray.Length; i++)
            {
                var euler = TestData.EulerArray[i];
                _element.transform.rotation = Quat.Euler(euler);
                _unityChild.transform.rotation = Quaternion.Euler(euler.ToVector());
                
                var enkluForward = _engine.Run<Vec3>("element.transform.forward");
                var unityForward = _unityChild.transform.forward;
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
            
            Assert.IsTrue(unity.Approximately(point, TOLERANCE));
        }
        
        [Test]
        public void InverseTransformPoint()
        {
            _element.transform.position = new Vec3(1, 2, 3);
            _element.transform.rotation = Quat.Euler(0, 90, 0);
            _element.transform.scale = new Vec3(3, -1, 0.5f);
            
            var point = _engine.Run<Vec3>(@"
                element.transform.inverseTransformPoint(v.create(4, -5, 6));
            ");

            var widget = (Widget) _element.Element;

            var unity = widget.GameObject.transform.InverseTransformPoint(4, -5, 6).ToVec();
            
            Assert.IsTrue(unity.Approximately(point, TOLERANCE));
        }

        private bool CompareVectors(Vector3 expected, Vec3 actual)
        {
            var equal = expected.Approximately(actual, TOLERANCE);

            if (!equal)
            {
                Log.Error(this, "Expected: {0}  Actual: {1}", expected, actual);
            }

            return equal;
        }
    }    
}
