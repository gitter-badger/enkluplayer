using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using NUnit.Framework;
using ElementSchemaData = CreateAR.EnkluPlayer.IUX.ElementSchemaData;

namespace CreateAR.EnkluPlayer.Test.UI
{
    [TestFixture]
    public class ElementSchema_Tests
    {
        private ElementSchema _schema;

        [SetUp]
        public void SetUp()
        {
            _schema = new ElementSchema("Foo");
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
        public void WrapSelf()
        {
            Assert.Throws<Exception>(() =>
            {
                _schema.Wrap(_schema);
            });
        }

        [Test]
        public void WrapCycle()
        {
            var a = new ElementSchema();
            var b = new ElementSchema();
            var c = new ElementSchema();

            c.Wrap(b);
            b.Wrap(a);

            Assert.Throws<Exception>(() =>
            {
                a.Wrap(c);
            });
        }

        [Test]
        public void WrapInt()
        {
            var state = new ElementSchema("Foo");
            state.Set("bar", 17);

            _schema.Wrap(state);

            Assert.AreEqual(17, _schema.Get<int>("bar").Value);
        }

        [Test]
        public void WrapIntSet()
        {
            var state = new ElementSchema("Foo");
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
            var state = new ElementSchema("Foo");
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
            var child = new ElementSchema("Foo");
            child.Wrap(_schema);

            var grandChild = new ElementSchema("Foo");
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
            var child = new ElementSchema("Foo");
            child.Wrap(_schema);

            var grandChild = new ElementSchema("Foo");
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
            var prop = _schema.Get<int>("bar");

            var a = new ElementSchema("Foo");
            a.Set("bar", 15);

            var b = new ElementSchema("Foo");
            b.Set("bar", 43);

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
                },
                Colors = new Dictionary<string, Col4>
                {
                    {"col", new Col4(4, 3, 2, 1)}
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

            var color = _schema.Get<Col4>("col").Value;
            Assert.AreEqual(4, color.r);
            Assert.AreEqual(3, color.g);
            Assert.AreEqual(2, color.b);
            Assert.AreEqual(1, color.a);
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

        [Test]
        public void HasProp()
        {
            Assert.IsTrue(_schema.HasProp("foo"));
            
            Assert.IsFalse(_schema.HasProp("bar"));
            _schema.Set("bar", 100);
            Assert.IsTrue(_schema.HasProp("bar"));
        }
        
        [Test]
        public void HasPropChild()
        {
            var schema = new ElementSchema("Foo");
            schema.Wrap(_schema);
            
            Assert.IsTrue(schema.HasProp("foo"));
            
            Assert.IsFalse(schema.HasProp("bar"));
            _schema.Set("bar", 100);
            Assert.IsTrue(schema.HasProp("bar"));
        }

        [Test]
        public void HasOwnProp()
        {
            var schema = new ElementSchema("Foo");
            schema.Wrap(_schema);
            
            Assert.IsTrue(_schema.HasOwnProp("foo"));
            Assert.IsFalse(schema.HasOwnProp("foo"));
        }

        [Test]
        public void HasOwnPropLinked()
        {
            var schema = new ElementSchema("Foo");
            schema.Wrap(_schema);

            schema.Get<int>("foo");
            
            Assert.IsTrue(_schema.HasOwnProp("foo"));
            Assert.IsFalse(schema.HasOwnProp("foo"));
        }
        
        [Test]
        public void HasOwnPropUnLinked()
        {
            var schema = new ElementSchema("Foo");
            schema.Wrap(_schema);

            var prop = schema.Get<int>("foo");
            prop.Value = 12;
            
            Assert.IsTrue(_schema.HasOwnProp("foo"));
            Assert.IsTrue(schema.HasOwnProp("foo"));
        }

        [Test]
        public void IterateProps()
        {
            var num = 0;
            var foo = false;
            foreach (var prop in _schema)
            {
                num++;

                if (prop.Name == "foo")
                {
                    foo = true;
                }
            }

            Assert.AreEqual(1, num);
            Assert.IsTrue(foo);
        }

        [Test]
        public void IteratePropsAdd()
        {
            // create prop
            _schema.GetOwn("bar", 2);

            var num = 0;
            var bar = false;
            foreach (var prop in _schema)
            {
                num++;

                if (prop.Name == "bar")
                {
                    bar = true;
                }
            }

            Assert.AreEqual(2, num);
            Assert.IsTrue(bar);
        }

        [Test]
        public void IterateIgnoreParent()
        {
            var schema = new ElementSchema("Foo");
            schema.GetOwn("bar", 2);
            schema.Wrap(_schema);

            var numProps = 0;
            foreach (var prop in schema)
            {
                numProps++;

                Assert.AreEqual("bar", prop.Name);
            }

            Assert.AreEqual(1, numProps);
        }

        [Test]
        public void IteratePropsOrder()
        {
            // create new prop
            _schema.GetOwn("bar", 2);

            var num = 0;
            var foo = false;
            var bar = false;
            foreach (var prop in _schema)
            {
                num++;

                if (prop.Name == "foo")
                {
                    foo = true;
                }
                else if (prop.Name == "bar")
                {
                    bar = true;

                    Assert.IsTrue(foo);
                }
            }

            Assert.AreEqual(2, num);
            Assert.IsTrue(bar);
            Assert.IsTrue(foo);
        }

        [Test]
        public void IterateChildProps()
        {
            var a = new ElementSchema("Foo");
            a.Set("a", "a");

            var b = new ElementSchema("Foo");
            b.Load(new ElementSchemaData
            {
                Strings =
                {
                    { "b", "b" }
                }
            });
            b.Wrap(a);

            var c = new ElementSchema("Foo");
            c.Load(new ElementSchemaData
            {
                Strings =
                {
                    { "c", "c" }
                }
            });
            c.Wrap(b);
            
            var propsC = c.ToArray();
            Assert.AreEqual(1, propsC.Length);
            Assert.AreEqual("c", ((ElementSchemaProp<string>) propsC[0]).Value);

            var propsB = b.ToArray();
            Assert.AreEqual(1, propsB.Length);
            Assert.AreEqual("b", ((ElementSchemaProp<string>)propsB[0]).Value);

            var propsA = a.ToArray();
            Assert.AreEqual(1, propsA.Length);
            Assert.AreEqual("a", ((ElementSchemaProp<string>)propsA[0]).Value);
        }

        [Test]
        public void Inherit()
        {
            var a = new ElementSchema();
            a.Set("a", "a");

            var b = new ElementSchema();
            b.Set("b", "b");
            b.Inherit(a);

            var c = new ElementSchema();
            c.Set("c", "c");
            c.Inherit(b);

            var propsC = c.ToArray();
            Assert.AreEqual(3, propsC.Length);

            var cA = c.Get<string>("a");
            var aA = a.Get<string>("a");

            aA.Value = "A";

            Assert.AreNotEqual(cA.Value, aA.Value);
        }

        [Test]
        public void InheritExisting()
        {
            var a = new ElementSchema();
            a.Set("a", "a");

            var b = new ElementSchema();
            b.Set("a", "b");
            b.Inherit(a);

            Assert.AreEqual("b", b.Get<string>("a").Value);
        }

        [Test]
        public void NewPropEvent()
        {
            var called = false;

            var a = new ElementSchema();
            a.OnSelfPropAdded += (name, type) =>
            {
                called = true;

                Assert.AreEqual("foo", name);
                Assert.AreEqual(typeof(string), type);
            };

            a.Set("foo", "test");

            Assert.IsTrue(called);
        }

        [Test]
        public void NewPropEventGetOwn()
        {
            var called = false;

            var a = new ElementSchema();
            a.OnSelfPropAdded += (name, type) =>
            {
                called = true;

                Assert.AreEqual("foo", name);
                Assert.AreEqual(typeof(string), type);
            };

            a.GetOwn("foo", "tball");

            Assert.IsTrue(called);
        }

        [Test]
        public void NewPropEventNotCalled()
        {
            var called = false;

            var a = new ElementSchema();
            a.Set("foo", "bar");

            a.OnSelfPropAdded += (name, type) =>
            {
                called = true;
            };

            a.Set("foo", "test");

            Assert.IsFalse(called);
        }

        [Test]
        public void GetOwnUpdate()
        {
            var called = false;

            var a = new ElementSchema();
            a.GetOwn("foo", "a").OnChanged += (_, __, next) =>
            {
                called = true;
                
                Assert.AreEqual(next, "b");
            };
            
            a.Set("foo", "b");

            Assert.IsTrue(called);
        }
    }
}