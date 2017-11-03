using System;
using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.UI
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

        [Test]
        public void LoadData()
        {
            _schema.Load(new ElementSchemaData
            {
                Ints = new Dictionary<string, int>
                {
                    {"int", 5}
                },
                Floats = new Dictionary<string, float>
                {
                    {"float", 5f}
                },
                Bools = new Dictionary<string, bool>
                {
                    {"bool", true}
                },
                Strings = new Dictionary<string, string>
                {
                    {"string", "foo"}
                },
                Vectors = new Dictionary<string, Vec3>
                {
                    {"vec", new Vec3(1, 2, 3)}
                }
            });

            Assert.AreEqual(5, _schema.Get<int>("int").Value);
            Assert.AreEqual(5f, _schema.Get<float>("float").Value);
            Assert.AreEqual(true, _schema.Get<bool>("bool").Value);
            Assert.AreEqual("foo", _schema.Get<string>("string").Value);

            var vec = _schema.Get<Vec3>("vec").Value;
            Assert.AreEqual(1, vec.x);
            Assert.AreEqual(2, vec.y);
            Assert.AreEqual(3, vec.z);
        }
    }
}