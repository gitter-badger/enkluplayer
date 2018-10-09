using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    [TestFixture]
    public class ElementJs_Tests
    {
        /// <summary>
        /// Verify ElementJs instances check equality based on their underlying Elements
        /// </summary>
        [Test]
        public void TestEquality()
        {
            Element elementA = new Element();
            Element elementB = new Element();

            ElementJs elementJsA1 = null;
            ElementJs elementJsA2 = null;

            // Test nulls
            Assert.IsTrue(elementJsA1 == elementJsA2);
            Assert.IsFalse(elementJsA1 != elementJsA2);

            elementJsA1 = new ElementJs(null, null, elementA);
            elementJsA2 = new ElementJs(null, null, elementA);
            ElementJs elementJsB = new ElementJs(null, null, elementB);
            
            // ==
            Assert.IsTrue(elementJsA1 == elementJsA2);
            Assert.IsFalse(elementJsA1 == elementJsB);
            Assert.IsFalse(elementJsA1 == null);

            // !=
            Assert.IsTrue(elementJsA1 != elementJsB);
            Assert.IsFalse(elementJsA1 != elementJsA2);
            Assert.IsTrue(elementJsA1 != null);

            // .Equals
            Assert.IsTrue(elementJsA1.Equals(elementJsA2));
            Assert.IsFalse(elementJsA1.Equals(elementJsB));
            Assert.IsFalse(elementJsA1.Equals(null));
        }
    }
}
