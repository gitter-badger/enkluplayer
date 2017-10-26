using NUnit.Framework;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class Vector3Js_Tests
    {
        [Test]
        public void Constructors()
        {
            var x = 12;
            var y = 14;
            var z = 3853;
            var source = new Vector3(x, y, z);

            var vecComp = new Vector3Js(x, y, z);
            var vec = new Vector3Js(source);

            Assert.AreEqual(x, vecComp.x);
            Assert.AreEqual(y, vecComp.y);
            Assert.AreEqual(z, vecComp.z);

            Assert.AreEqual(source.x, vec.x);
            Assert.AreEqual(source.y, vec.y);
            Assert.AreEqual(source.z, vec.z);
        }
    }
}