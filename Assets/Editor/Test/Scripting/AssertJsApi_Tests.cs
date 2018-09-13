using System;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class AssertJsApi_Tests : JsTestBase
    {
        [RuntimeTest]
        public void Methods()
        {
            Run("assert.areEqual(1, 1, 'Error!');");
            Assert.Throws<Exception>(() => Run("assert.areEqual(1, 2, 'AreEqual!');"));

            Run("assert.isTrue(true, 'Error!');");
            Assert.Throws<Exception>(() => Run("assert.isTrue(false, 'IsTrue!');"));

            Run("assert.isNull(null, 'Error!');");
            Assert.Throws<Exception>(() => Run("assert.isNull({}, 'IsNull!');"));
        }
    }
}