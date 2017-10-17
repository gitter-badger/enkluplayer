using System;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test
{
    [TestFixture]
    public class ContentGraph_Tests
    {
        private ContentGraph _graph;

        private readonly HierarchyNodeData _root = new HierarchyNodeData
        {
            Id = "a",
            ContentId = "A",
            Children = new []
            {
                new HierarchyNodeData
                {
                    Id = "b",
                    ContentId = "B",
                    Children = new []
                    {
                        new HierarchyNodeData
                        {
                            Id = "bb",
                            ContentId = "BB"
                        },
                        new HierarchyNodeData
                        {
                            Id = "bbb",
                            ContentId = "BBB"
                        }
                    }
                },
                new HierarchyNodeData
                {
                    Id = "c",
                    ContentId = "C"
                }
            }
        };

        [SetUp]
        public void Setup()
        {
            _graph = new ContentGraph();
            _graph.Load(_root);
        }

        [Test]
        public void Clear()
        {
            Assert.IsNotNull(_graph.Root);

            _graph.Clear();

            Assert.IsNull(_graph.Root);
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
        public void WatchEvents()
        {
            var called = false;

            _graph.OnLoaded += root =>
            {
                called = true;

                Assert.AreSame("a", root.Id);
            };

            _graph.Load(_root);
            
            Assert.IsTrue(called);
        }

        [Test]
        public void WatchNodeUpdate()
        {
            var called = false;
            var b = _graph.FindOne("b");

            b.OnUpdate += node =>
            {
                called = true;

                Assert.AreSame(b, node);

                // make sure the update actually updated
                Assert.AreEqual(1, node.Children.Count);
                Assert.AreEqual("bb", node.Children[0].Id);
                Assert.AreEqual("d", node.Children[0].Children[0].Id);
            };

            _graph.Update(new HierarchyNodeData
            {
                Id = "b",
                ContentId = "B",
                Children = new []
                {
                    new HierarchyNodeData
                    {
                        Id = "bb",
                        ContentId = "BB",
                        Children = new []
                        {
                            new HierarchyNodeData
                            {
                                Id = "d",
                                ContentId = "D"
                            }
                        }
                    }
                }
            });

            Assert.IsTrue(called);
        }

        [Test]
        public void WatchNodeRemovedUpdate()
        {
            var called = false;
            var bbb = _graph.FindOne("bbb");

            bbb.OnRemove += node =>
            {
                called = true;

                Assert.AreSame(bbb, node);
            };

            _graph.Update(new HierarchyNodeData
            {
                Id = "b",
                ContentId = "B"
            });

            Assert.IsTrue(called);
        }

        [Test]
        public void WatchNodePropogate()
        {
            throw new NotImplementedException();
        }
    }
}