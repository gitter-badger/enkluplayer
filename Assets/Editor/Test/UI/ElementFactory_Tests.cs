using System;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class ElementFactory_Tests
    {
        private ElementFactory _elements;

        [SetUp]
        public void Setup()
        {
            _elements = new ElementFactory();
        }

        [Test]
        public void CreateElement()
        {
            var id = Guid.NewGuid().ToString();
            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = id
                },
                Elements = new []
                {
                    new ElementData
                    {
                        Id = id
                    }
                }
            });

            Assert.AreEqual(id, element.Id);
        }
    }
}