using System;
using CreateAR.SpirePlayer.Vine;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Vine
{
    [TestFixture]
    public class VineEngine_Tests
    {
        private VineEngine _engine;

        [SetUp]
        public void Setup()
        {
            _engine = new VineEngine();
        }

        [Test]
        public void MultipleRoots()
        {
            Assert.Throws<Exception>(() =>
            {
                _engine.Parse(@"
                    <?Vine>
                    <Container />
                    <Container />");
            });
        }

        [Test]
        public void Element()
        {
            var description = _engine.Parse(@"
<?Vine>
<Container></Container>");

            Assert.AreEqual(ElementTypes.CONTAINER, description.Elements[0].Type);
        }

        [Test]
        public void Element_Child()
        {
            var description = _engine.Parse(@"
<?Vine>
<Container>
    <Container />
</Container>");

            Assert.AreEqual(ElementTypes.CONTAINER, description.Elements[0].Type);
            Assert.AreEqual(ElementTypes.CONTAINER, description.Elements[0].Children[0].Type);
        }

        [Test]
        public void Element_ChildTypes()
        {
            var description = _engine.Parse(@"
<?Vine>
<Container>
    <Container />
    <Menu />
    <Button />
    <Caption />
    <Cursor />
</Container>");

            Assert.AreEqual(ElementTypes.CONTAINER, description.Elements[0].Children[0].Type);
            Assert.AreEqual(ElementTypes.MENU, description.Elements[0].Children[1].Type);
            Assert.AreEqual(ElementTypes.BUTTON, description.Elements[0].Children[2].Type);
            Assert.AreEqual(ElementTypes.CAPTION, description.Elements[0].Children[3].Type);
            Assert.AreEqual(ElementTypes.CURSOR, description.Elements[0].Children[4].Type);
        }

        [Test]
        public void Element_SelfClosing()
        {
            var description = _engine.Parse(@"
<?Vine>
<Container />");

            Assert.AreEqual(ElementTypes.CONTAINER, description.Elements[0].Type);
        }

        [Test]
        public void Element_BadElement()
        {
            Assert.Throws<Exception>(() =>
            {
                _engine.Parse(@"
<?Vine>
<Foo></Foo>");
            });
        }

        [Test]
        public void Element_SelfClosing_BadElement()
        {
            Assert.Throws<Exception>(() =>
            {
                _engine.Parse(@"
<?Vine>
<Foo />");
            });
        }
    }
}