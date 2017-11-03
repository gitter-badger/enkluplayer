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
        public void RemoveChild()
        {
            var element = new Element();
            _root.AddChild(element);
            _root.RemoveChild(element);

            Assert.AreEqual(0, _root.Children.Length);
        }
    }
}