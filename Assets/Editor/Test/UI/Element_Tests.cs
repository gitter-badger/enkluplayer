using CreateAR.SpirePlayer.UI;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.UI
{
    [TestFixture]
    public class Element_Tests
    {
        private Element _root;

        [SetUp]
        public void Setup()
        {
            _root = new Element();
        }

        [Test]
        public void AddChild()
        {
            var element = new Element();
            _root.AddChild(element);

            Assert.AreSame(element, _root.Children[0]);
        }

        [Test]
        public void AddChildUnique()
        {
            var element = new Element();
            _root.AddChild(element);
            _root.AddChild(element);

            Assert.AreEqual(1, _root.Children.Length);
        }

        [Test]
        public void AddChildOrdered()
        {
            var a = new Element();
            var b = new Element();
            _root.AddChild(a);
            _root.AddChild(b);

            Assert.AreSame(a, _root.Children[0]);
            Assert.AreSame(b, _root.Children[1]);
        }

        [Test]
        public void AddChildUniqueOrdered()
        {
            var a = new Element();
            var b = new Element();

            _root.AddChild(a);
            _root.AddChild(b);
            _root.AddChild(a);

            Assert.AreSame(a, _root.Children[1]);
            Assert.AreSame(b, _root.Children[0]);
        }

        [Test]
        public void AddChildEvent()
        {
            var isCalled = false;
            var child = new Element();

            _root.OnChildAdded += (root, value) =>
            {
                isCalled = true;
                Assert.AreSame(child, value);
            };
            _root.AddChild(child);

            Assert.IsTrue(isCalled);
        }

        [Test]
        public void RemoveChild()
        {
            var element = new Element();
            _root.AddChild(element);

            Assert.IsTrue(_root.RemoveChild(element));
            Assert.AreEqual(0, _root.Children.Length);
        }

        [Test]
        public void RemoveChildTwice()
        {
            var element = new Element();
            _root.AddChild(element);

            Assert.IsTrue(_root.RemoveChild(element));
            Assert.IsFalse(_root.RemoveChild(element));
        }

        [Test]
        public void RemoveNonChild()
        {
            var element = new Element();

            Assert.IsFalse(_root.RemoveChild(element));
        }

        [Test]
        public void RemoveChildEvent()
        {
            var isCalled = false;
            var child = new Element();
            _root.AddChild(child);

            _root.OnChildRemoved += (root, value) =>
            {
                isCalled = true;

                Assert.AreSame(child, value);
            };

            _root.RemoveChild(child);

            Assert.IsTrue(isCalled);
        }
    }
}