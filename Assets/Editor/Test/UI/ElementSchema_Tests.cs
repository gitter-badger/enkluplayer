using System;
using CreateAR.SpirePlayer.UI;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class ElementSchema_Tests
    {
        private ElementSchema _schema;

        [SetUp]
        public void SetUp()
        {
            _schema = new ElementSchema();
            _schema.Set("foo", 5);
        }
        
        [Test]
        public void GetPropInt()
        {
            Assert.AreEqual(5, _schema.Get<int>("foo").Value);
        }

        [Test]
        public void SetPropInt()
        {
            var prop = _schema.Get<int>("foo");
            Assert.AreEqual(5, prop.Value);

            _schema.Set("foo", 12);

            Assert.AreEqual(12, prop.Value);
        }

        /// <summary>
        /// Demonstrates that once set, the type cannot be changed.
        /// </summary>
        [Test]
        public void GetPropChangeType()
        {
            Assert.AreEqual(false, _schema.Get<bool>("foo").Value);

            _schema.Set("foo", true);

            Assert.AreEqual(false, _schema.Get<bool>("foo").Value);
        }

        [Test]
        public void WrapInt()
        {
            var state = new ElementSchema();
            state.Set("bar", 17);

            _schema.Wrap(state);

            Assert.AreEqual(17, _schema.Get<int>("bar").Value);
        }

        [Test]
        public void WrapIntSet()
        {
            var state = new ElementSchema();
            state.Set("bar", 17);

            _schema.Wrap(state);
            _schema.Set("bar", 4);

            Assert.AreEqual(4, _schema.Get<int>("bar").Value);
            Assert.AreEqual(17, state.Get<int>("bar").Value);
        }

        [Test]
        public void WrapOnce()
        {
            _schema.Wrap(new ElementSchema());

            Assert.Throws<ArgumentException>(() => _schema.Wrap(new ElementSchema()));
        }
    }
}