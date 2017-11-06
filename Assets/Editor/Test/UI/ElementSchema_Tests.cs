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

        [Test]
        public void SetPropIntEvent()
        {
            var isCalled = false;
            var prop = _schema.Get<int>("foo");
            prop.OnChanged += (property, oldValue, newValue) =>
            {
                isCalled = true;

                Assert.AreSame(prop, property);
                Assert.AreEqual(5, oldValue);
                Assert.AreEqual(12, newValue);
            };

            _schema.Set("foo", 12);

            Assert.IsTrue(isCalled);
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
        public void ChildSetEvent()
        {
            var calls = 0;

            // wrap
            var state = new ElementSchema();
            state.Set("bar", 17);
            var parentBar = state.Get<int>("bar");
            _schema.Wrap(state);

            // listen to child prop
            var bar = _schema.Get<int>("bar");
            bar.OnChanged += (prop, prev, next) =>
            {
                calls++;
            };

            // setting parent prop should change child prop
            parentBar.Value = 1776;
            Assert.AreEqual(1776, bar.Value);

            // setting child prop should break connection
            bar.Value = 21;
            Assert.AreEqual(21, bar.Value);
            Assert.AreEqual(1776, parentBar.Value);

            // setting parent prop should not trigger
            parentBar.Value = 100;
            Assert.AreEqual(21, bar.Value);
            Assert.AreEqual(100, parentBar.Value);

            // should have received two total calls
            Assert.AreEqual(2, calls);
        }

        [Test]
        public void GrandChildSetEvent()
        {
            var child = new ElementSchema();
            child.Wrap(_schema);

            var grandChild = new ElementSchema();
            grandChild.Wrap(child);

            var bar = grandChild.Get<int>("bar");
            bar.OnChanged += (prop, prev, next) =>
            {
                Assert.AreSame(prop, bar);
                Assert.AreEqual(prev, 0);
                Assert.AreEqual(next, 47);
            };

            _schema.Set("bar", 47);
        }

        [Test]
        public void GrandChildSetChildEvent()
        {
            var child = new ElementSchema();
            child.Wrap(_schema);

            var grandChild = new ElementSchema();
            grandChild.Wrap(child);

            var bar = grandChild.Get<int>("bar");
            bar.OnChanged += (prop, prev, next) =>
            {
                Assert.AreSame(prop, bar);
                Assert.AreEqual(prev, 0);
                Assert.AreEqual(next, 47);
            };

            child.Set("bar", 47);
        }
        
        [Test]
        public void WrapReparent()
        {
            var prop = _schema.Get<int>("foo");

            var a = new ElementSchema();
            a.Set("foo", 15);

            var b = new ElementSchema();
            b.Set("foo", 43);

            _schema.Wrap(a);
            Assert.AreEqual(15, prop.Value);

            _schema.Wrap(b);
            Assert.AreEqual(43, prop.Value);

            // break connection
            prop.Value = 20;
            _schema.Wrap(a);
            Assert.AreEqual(20, prop.Value);
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

        [Test]
        public void SetValue()
        {
            var foo = _schema.Get<int>("foo");
            foo.Value = 12;

            Assert.AreEqual(12, foo.Value);
        }

        [Test]
        public void SetValueEvent()
        {
            var isCalled = false;
            var foo = _schema.Get<int>("foo");

            foo.OnChanged += (prop, prev, next) =>
            {
                isCalled = true;

                Assert.AreSame(foo, prop);
                Assert.AreEqual(5, prev);
                Assert.AreEqual(12, next);
            };

            foo.Value = 12;

            Assert.IsTrue(isCalled);
        }
    }
}