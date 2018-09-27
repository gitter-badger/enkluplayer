using System;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using NUnit.Framework;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class QuatJSApi_Tests
    {
        private Engine _engine;

        [SetUp]
        public void Setup()
        {
            _engine = new Engine(options => {
                options.CatchClrExceptions(exception => { throw exception; });
                options.AllowClr();
            });

            _engine.SetValue("q", QuatMethods.Instance);
            _engine.SetValue("quat", new Func<float, float, float, float, Quat>(QuatMethods.create));
        }

        [Test]
        public void Quat()
        {
            var result = _engine.Run<Quat>("quat(-0.2, 0.9, -0.1, 0.2)");

            Assert.IsTrue(Math.Abs(-0.2f - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(0.9f  - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(-0.1f - result.z) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(0.2f  - result.w) < Mathf.Epsilon);

            result = _engine.Run<Quat>("quat(-0.2, 0.9, -0.1, 0.2).set(0.6, -0.3, 0.2, -0.1)");

            Assert.IsTrue(Math.Abs(0.6f  - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(-0.3f - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(0.2f  - result.z) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(-0.1f - result.w) < Mathf.Epsilon);
        }

        [Test]
        public void QuatEuler()
        {
            // Test our Euler -> Quat matches Unity's Vector3 -> Quaternion over a variety of rotations
            for (int i = 0; i < TestData.EulerArray.Length; i++)
            {
                var euler = TestData.EulerArray[i];
                var quaternion = Quaternion.Euler(euler.x, euler.y, euler.z);
                var result = _engine.Run<Quat>(
                    string.Format("q.euler({0}, {1}, {2})", euler.x, euler.y, euler.z));

                var equal = Mathf.Approximately(quaternion.x, result.x)
                         && Mathf.Approximately(quaternion.y, result.y)
                         && Mathf.Approximately(quaternion.z, result.z)
                         && Mathf.Approximately(quaternion.w, result.w);

                if (!equal)
                {
                    string quatStr = string.Format("({0:0.00}, {1:0.00}, {2:0.00}, {3:0.00})", 
                        quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                    Log.Error(this, euler + " " + quatStr + " " + result);
                }

                Assert.IsTrue(equal);
            }
        }
    }
}