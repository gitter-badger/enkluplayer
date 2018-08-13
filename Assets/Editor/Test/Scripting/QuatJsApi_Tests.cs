using System;
using UnityEngine;
using NUnit.Framework;
using Jint;
using Jint.Native;

namespace CreateAR.SpirePlayer.Test.Scripting
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

            Assert.IsTrue(result.GetType() == typeof(Quat));
            Assert.IsTrue(Math.Abs(-0.2f - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(0.9f  - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(-0.1f - result.z) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(0.2f  - result.w) < Mathf.Epsilon);
        }

        [Test]
        public void QuatEuler()
        {
            var quaternion = Quaternion.Euler(30, 90, 0);
            var result = _engine.Run<Quat>("q.euler(30, 90, 0)");

            Assert.IsTrue(Math.Abs(quaternion.x - result.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(quaternion.y - result.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(quaternion.z - result.z) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(quaternion.w - result.w) < Mathf.Epsilon);
        }
    }
}