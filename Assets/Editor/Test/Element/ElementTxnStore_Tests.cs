using System.Collections.Generic;
using System.Text.RegularExpressions;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Test.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CreateAR.SpirePlayer.Test.Txn
{
    [TestFixture]
    public class ElementTxnStore_Tests
    {
        private ElementTxnStore _store;
        private Element _root;
        private uint _ids = 1000;

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

        private uint Id()
        {
            return _ids++;
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

            var txn = new ElementTxn(Id())
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
            var txn = new ElementTxn(Id())
                .Create("root", "aa", 0)
                .Create("b", "c", 0);

            _store.Apply(txn);

            var aa = _root.FindOne<Element>("aa");
            Assert.AreEqual("root", aa.Parent.Id);

            var c = _root.FindOne<Element>("..c");
            Assert.AreEqual("b", c.Parent.Id);
        }

        [Test]
        public void ApplyDelete()
        {
            var txn = new ElementTxn(Id()).Delete("b");

            _store.Apply(txn);

            Assert.IsNull(_root.FindOne<Element>("..b"));
        }

        [Test]
        public void ApplyAll()
        {
            var txn = new ElementTxn(Id())
                .Create("a", "aa", 0)
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

            var txn = new ElementTxn(Id());
            txn.Actions.Add(new ElementActionData());
            txn.Create("a", "aa", 0);

            _store.Apply(txn);

            // should not have created "aa"
            Assert.IsNull(_root.FindOne<Element>("..aa"));
        }

        [Test]
        public void RequestAndCommit()
        {
            var txn = new ElementTxn(Id())
                .Create("a", "aa", 0)
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
            var txn = new ElementTxn(Id()).Update("b", "foo", "bar");

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
            var txn = new ElementTxn(Id()).Update("b", "foo", "bar");

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