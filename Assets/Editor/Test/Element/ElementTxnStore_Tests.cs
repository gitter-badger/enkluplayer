using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Test.UI;
using Enklu.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CreateAR.EnkluPlayer.Test.Txn
{
    [TestFixture]
    public class ElementTxnStore_Tests
    {
        private ElementTxnStore _store;
        private Element _root;

        [SetUp]
        public void Setup()
        {
            var factory = new DummyElementFactory();
            _root = factory.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = "root"
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = "root",
                        Children = new []
                        {
                            new ElementData
                            {
                                Id = "a",
                                Children = new []
                                {
                                    new ElementData
                                    {
                                        Id = "b",
                                        Schema = new ElementSchemaData
                                        {
                                            Strings = new Dictionary<string, string>
                                            {
                                                { "foo", "buzz" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            _store = new ElementTxnStore(new ElementActionStrategy(
                factory,
                _root));
        }

        [Test]
        public void ApplyUpdates()
        {
            var intVal = 12;
            var floatVal = 12f;
            var stringVal = "boop";
            var boolVal = true;
            var vecVal = new Vec3(0.1f, 2, 34.5f);
            var colVal = new Col4(0.2f, 12, 345f, 1);

            var txn = new ElementTxn("test")
                .Update("a", "foo", intVal)
                .Update("a", "foo", floatVal)
                .Update("a", "foo", stringVal)
                .Update("a", "foo", boolVal)
                .Update("a", "foo", vecVal)
                .Update("a", "foo", colVal);

            _store.Apply(txn);

            var a = _root.FindOne<Element>("..a");

            Assert.AreEqual(intVal, a.Schema.Get<int>("foo").Value);
            Assert.AreEqual(floatVal, a.Schema.Get<float>("foo").Value);
            Assert.AreEqual(stringVal, a.Schema.Get<string>("foo").Value);
            Assert.AreEqual(boolVal, a.Schema.Get<bool>("foo").Value);
            Assert.AreEqual(vecVal, a.Schema.Get<Vec3>("foo").Value);
            Assert.AreEqual(colVal, a.Schema.Get<Col4>("foo").Value);
        }

        [Test]
        public void ApplyCreate()
        {
            var txn = new ElementTxn("test")
                .Create("root", new ElementData
                {
                    Id = "aa"
                })
                .Create("b", new ElementData
                {
                    Id = "b"
                });

            _store.Apply(txn);

            var aa = _root.FindOne<Element>("aa");
            Assert.AreEqual("root", aa.Parent.Id);

            var b = _root.FindOne<Element>("..b");
            Assert.AreEqual("a", b.Parent.Id);
        }

        [Test]
        public void ApplyDelete()
        {
            var txn = new ElementTxn("test").Delete("b");

            _store.Apply(txn);

            Assert.IsNull(_root.FindOne<Element>("..b"));
        }

        [Test]
        public void ApplyMove()
        {
            var newLocal = new Vec3(1, 12, 0);
            var newRot = new Vec3(17, 3, -123);
            var newScale = new Vec3(10, 33, 1);
            var txn = new ElementTxn("test").Move("b", "root", newLocal, newRot, newScale);

            _store.Apply(txn);

            var b = _root.FindOne<Element>("..b");
            Assert.AreEqual("root", b.Parent.Id);

            var pos = b.Schema.Get<Vec3>("position").Value;
            Assert.IsTrue(Math.Abs(newLocal.x - pos.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(newLocal.y - pos.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(newLocal.z - pos.z) < Mathf.Epsilon);

            var rot = b.Schema.Get<Vec3>("rotation").Value;
            Assert.IsTrue(Math.Abs(newRot.x - rot.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(newRot.y - rot.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(newRot.z - rot.z) < Mathf.Epsilon);

            var scale = b.Schema.Get<Vec3>("scale").Value;
            Assert.IsTrue(Math.Abs(newScale.x - scale.x) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(newScale.y - scale.y) < Mathf.Epsilon);
            Assert.IsTrue(Math.Abs(newScale.z - scale.z) < Mathf.Epsilon);
        }

        [Test]
        public void ApplyAll()
        {
            var txn = new ElementTxn("test")
                .Create("a", new ElementData
                {
                    Id = "aa"
                })
                .Update("aa", "foo", "bar")
                .Delete("b");

            _store.Apply(txn);

            var aa = _root.FindOne<Element>("..aa");

            Assert.AreSame("bar", aa.Schema.Get<string>("foo").Value);
            Assert.IsNull(_root.FindOne<Element>("..b"));
        }

        [Test]
        public void ApplyBad()
        {
            LogAssert.Expect(LogType.Error, new Regex("Invalid action type"));

            var txn = new ElementTxn("test");
            txn.Actions.Add(new ElementActionData());
            txn.Create("a", new ElementData
            {
                Id = "aa"
            });

            _store.Apply(txn);

            // should not have created "aa"
            Assert.IsNull(_root.FindOne<Element>("..aa"));
        }

        [Test]
        public void RequestAndCommit()
        {
            var txn = new ElementTxn("test")
                .Create("a", new ElementData
                {
                    Id = "aa"
                })
                .Update("aa", "foo", "bar");

            string error;

            Assert.IsTrue(_store.Request(txn, out error));
            Assert.IsTrue(string.IsNullOrEmpty(error));

            Assert.IsNull(_root.FindOne<Element>("..aa"));

            _store.Commit(txn.Id);

            var aa = _root.FindOne<Element>("..aa");
            Assert.AreEqual("bar", aa.Schema.Get<string>("foo").Value);
        }

        [Test]
        public void RequestPreCommitAndCommit()
        {
            var txn = new ElementTxn("test").Update("b", "foo", "bar");

            string error;

            Assert.IsTrue(_store.Request(txn, out error));
            Assert.IsTrue(string.IsNullOrEmpty(error));

            var b = _root.FindOne<Element>("..b");

            Assert.AreEqual("bar", b.Schema.Get<string>("foo").Value);

            _store.Commit(txn.Id);

            Assert.AreEqual("bar", b.Schema.Get<string>("foo").Value);
        }

        [Test]
        public void RequestPreCommitAndRollback()
        {
            var txn = new ElementTxn("test").Update("b", "foo", "bar");

            string error;

            Assert.IsTrue(_store.Request(txn, out error));
            Assert.IsTrue(string.IsNullOrEmpty(error));

            var b = _root.FindOne<Element>("..b");

            Assert.AreEqual("bar", b.Schema.Get<string>("foo").Value);

            _store.Rollback(txn.Id);

            Assert.AreEqual("buzz", b.Schema.Get<string>("foo").Value);
        }
    }
}