using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class ContentGraph_Tests
    {
        private ContentGraph _graph;

        private static HierarchyNodeLocatorData[] Locators()
        {
            return new [] {
                new HierarchyNodeLocatorData
                {
                    Name = "__self__"
                }
            };
        }

        private readonly HierarchyNodeData _root = new HierarchyNodeData
        {
            Id = "a",
            ContentId = "A",
            Locators = Locators(),
            Children = new []
            {
                new HierarchyNodeData
                {
                    Id = "b",
                    ContentId = "B",
                    Locators = Locators(),
                    Children = new []
                    {
                        new HierarchyNodeData
                        {
                            Id = "bb",
                            ContentId = "BB",
                            Locators = Locators()
                        },
                        new HierarchyNodeData
                        {
                            Id = "bbb",
                            ContentId = "BBB",
                            Locators = Locators()
                        }
                    }
                },
                new HierarchyNodeData
                {
                    Id = "c",
                    ContentId = "C",
                    Locators = Locators()
                }
            }
        };

        [SetUp]
        public void Setup()
        {
            _graph = new ContentGraph();
            _graph.Add("root", _root);
        }

        [Test]
        public void Clear()
        {
            _graph.Clear();

            Assert.AreEqual(0, _graph.Root.Children.Count);
        }

        [Test]
        public void QueryGraph()
        {
            var c = _graph.FindOne("c");
            var bbb = _graph.FindOne("bbb");

            Assert.IsNotNull(c);
            Assert.AreEqual("c", c.Id);

            Assert.IsNotNull(bbb);
            Assert.AreEqual("bbb", bbb.Id);
        }

        [Test]
        public void QueryNode()
        {
            var b = _graph.FindOne("b");
            var bbb = b.FindOne("bbb");
            
            Assert.IsNotNull(bbb);
            Assert.AreEqual("bbb", bbb.Id);
            Assert.AreSame(b, b.FindOne("b"));
            Assert.IsNull(b.FindOne("c"));
        }

        [Test]
        public void QueryChild()
        {
            var a = _graph.FindOne("a");
            var b = a.Child("b");
            var bbb = a.Child("bbb");

            Assert.AreSame("b", b.Id);
            Assert.IsNull(bbb);
        }
        
        [Test]
        public void WatchNodeUpdate()
        {
            var called = false;
            var b = _graph.FindOne("b");
            b.OnUpdated += node =>
            {
                called = true;

                Assert.AreSame(b, node);
                Assert.AreEqual(0, node.Children.Count);
            };

            _graph.Update(new HierarchyNodeData
            {
                Id = "b",
                ContentId = "B"
            });

            Assert.IsTrue(called);
        }

        [Test]
        public void WatchNodeRemoved()
        {
            var called = false;
            var b = _graph.FindOne("b");
            b.OnRemoved += node =>
            {
                called = true;
            };

            _graph.Update(new HierarchyNodeData
            {
                Id = "a",
                ContentId = "A"
            });

            Assert.IsTrue(called);
        }

        [Test]
        public void WatchNodeChildUpdate()
        {
            var called = false;
            var a = _graph.FindOne("a");
            a.OnChildUpdated += (node, child) =>
            {
                called = true;

                Assert.AreSame(a, node);
                Assert.AreEqual("b", child.Id);
            };

            _graph.Update(new HierarchyNodeData
            {
                Id = "b",
                ContentId = "B"
            });

            Assert.IsTrue(called);
        }

        [Test]
        public void WatchNodeChildRemoved()
        {
            var called = 0;
            var a = _graph.FindOne("a");
            a.OnChildRemoved += (node, child) =>
            {
                called++;

                Assert.AreSame(a, node);
            };

            _graph.Update(new HierarchyNodeData
            {
                Id = "b",
                ContentId = "B"
            });

            Assert.AreEqual(2, called);
        }

        [Test]
        public void WatchNodeChildAdded()
        {
            var called = false;
            var a = _graph.FindOne("a");
            a.OnChildAdded += (node, child) =>
            {
                called = true;

                Assert.AreSame(a, node);
                Assert.AreSame("q", child.Id);
            };

            _graph.Update(new HierarchyNodeData
            {
                Id = "c",
                ContentId = "C",
                Children = new []
                {
                    new HierarchyNodeData
                    {
                        Id = "q",
                        ContentId = "Q",
                        Locators = Locators()
                    }
                }
            });

            Assert.IsTrue(called);
        }

        [Test]
        public void Add()
        {
            Assert.IsTrue(_graph.Add("a", new HierarchyNodeData
            {
                Id = "qq",
                Locators = Locators()
            }));

            Assert.IsNotNull(_graph.FindOne("qq"));
        }

        [Test]
        public void AddBad()
        {
            Assert.IsFalse(_graph.Add("foo", new HierarchyNodeData
            {
                Id = "qq",
                Locators = Locators()
            }));

            Assert.IsNull(_graph.FindOne("qq"));
        }
        
        [Test]
        public void AddChildAddedEvent()
        {
            var called = false;
            var a = _graph.FindOne("a");
            a.OnChildAdded += (node, child) =>
            {
                called = true;

                Assert.AreSame(a, node);
                Assert.AreEqual("qq", child.Id);
            };

            _graph.Add("a", new HierarchyNodeData
            {
                Id = "qq",
                Locators = Locators()
            });

            Assert.IsNotNull(_graph.FindOne("qq"));
            Assert.IsTrue(called);
        }

        [Test]
        public void Remove()
        {
            Assert.IsTrue(_graph.Remove("b"));
            Assert.IsNull(_graph.FindOne("b"));
        }

        [Test]
        public void RemoveRoot()
        {
            Assert.IsFalse(_graph.Remove(_graph.Root.Id));
        }

        [Test]
        public void RemoveBad()
        {
            Assert.IsFalse(_graph.Remove("foo"));
        }

        [Test]
        public void RemoveChildRemovedEvent()
        {
            var called = false;
            var a = _graph.FindOne("a");
            a.OnChildRemoved += (node, child) =>
            {
                called = true;

                Assert.AreSame(a, node);
                Assert.AreSame("bb", child.Id);
            };

            _graph.Remove("bb");

            Assert.IsTrue(called);
        }
    }
}