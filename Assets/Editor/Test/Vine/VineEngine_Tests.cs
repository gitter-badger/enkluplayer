using System;
using CreateAR.SpirePlayer.Vine;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.Vine
{
    [TestFixture]
    public class VineEngine_Tests
    {
        private VineImporter _importer;

        [SetUp]
        public void Setup()
        {
            _importer = new VineImporter();
        }

        [Test]
        public void MultipleRoots()
        {
            Assert.Throws<Exception>(() =>
            {
                _importer.Parse(@"
                    <?Vine>
                    <Container />
                    <Container />");
            });
        }

        [Test]
        public void Element()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container></Container>");

            Assert.AreEqual(ElementTypes.CONTAINER, description.Elements[0].Type);
        }

        [Test]
        public void Element_Child()
        {
            var description = _importer.Parse(@"
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
            var description = _importer.Parse(@"
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
        public void Element_Child_Deep()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container>
    <Menu>
        <Button>
            <Caption>
                <Cursor />
            </Caption>
        </Button>
    </Menu>
</Container>");

            Assert.AreEqual(ElementTypes.CURSOR,
                description
                    .Elements[0]
                    .Children[0]
                    .Children[0]
                    .Children[0]
                    .Children[0].Type);
        }

        [Test]
        public void Element_SelfClosing()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container />");

            Assert.AreEqual(ElementTypes.CONTAINER, description.Elements[0].Type);
        }

        [Test]
        public void Element_BadElement()
        {
            Assert.Throws<Exception>(() =>
            {
                _importer.Parse(@"
<?Vine>
<Foo></Foo>");
            });
        }

        [Test]
        public void Element_SelfClosing_BadElement()
        {
            Assert.Throws<Exception>(() =>
            {
                _importer.Parse(@"
<?Vine>
<Foo />");
            });
        }

        [Test]
        public void Element_Attributes_String()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container id='test_container'>
</Container>");

            Assert.AreEqual("test_container", description.Elements[0].Schema.Strings["id"]);
        }

        [Test]
        public void Element_Attributes_Int()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container foo=5>
</Container>");

            Assert.AreEqual(5, description.Elements[0].Schema.Ints["foo"]);
        }

        [Test]
        public void Element_Attributes_Float()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container foo=5.4>
</Container>");

            Assert.IsTrue(Math.Abs(5.4f - description.Elements[0].Schema.Floats["foo"]) < float.Epsilon);
        }

        [Test]
        public void Element_Attributes_Bool_True()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container foo=true>
</Container>");

            Assert.AreEqual(true, description.Elements[0].Schema.Bools["foo"]);
        }

        [Test]
        public void Element_Attributes_Bool_False()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container foo=false>
</Container>");

            Assert.AreEqual(false, description.Elements[0].Schema.Bools["foo"]);
        }
        
        [Test]
        public void Element_Attributes_Vec3()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container foo=(3.1, 2, 1)>
</Container>");

            var vec = description.Elements[0].Schema.Vectors["foo"];
            Assert.IsTrue(Math.Abs(3.1f - vec.x) < float.Epsilon);
            Assert.IsTrue(Math.Abs(2f - vec.y) < float.Epsilon);
            Assert.IsTrue(Math.Abs(1f - vec.z) < float.Epsilon);
        }

        [Test]
        public void Element_Attributes_Multi()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container
    foo='test_value'
    bar=5
    fizz=true
    buzz=5.4>
</Container>");

            Assert.AreEqual("test_value", description.Elements[0].Schema.Strings["foo"]);
            Assert.AreEqual(5, description.Elements[0].Schema.Ints["bar"]);
            Assert.IsTrue(description.Elements[0].Schema.Bools["fizz"]);
            Assert.IsTrue(Math.Abs(5.4f - description.Elements[0].Schema.Floats["buzz"]) < float.Epsilon);
        }

        [Test]
        public void Element_Attributes_Multi_SelfClosing()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container
    foo='test_value'
    bar=5
    fizz=true
    buzz=5.4 />");

            Assert.AreEqual("test_value", description.Elements[0].Schema.Strings["foo"]);
            Assert.AreEqual(5, description.Elements[0].Schema.Ints["bar"]);
            Assert.IsTrue(description.Elements[0].Schema.Bools["fizz"]);
            Assert.IsTrue(Math.Abs(5.4f - description.Elements[0].Schema.Floats["buzz"]) < float.Epsilon);
        }

        [Test]
        public void Element_Attributes_Multi_SameName()
        {
            var description = _importer.Parse(@"
<?Vine>
<Container foo='test_value' foo=5>
</Container>");

            Assert.AreEqual("test_value", description.Elements[0].Schema.Strings["foo"]);
            Assert.AreEqual(5, description.Elements[0].Schema.Ints["foo"]);
        }

        [Test]
        public void Element_Attributes_Multi_Collide()
        {
            Assert.Throws<Exception>(() => _importer.Parse(@"
<?Vine>
<Container foo='test_value' foo='another_value'>
</Container>"));
        }
    }
}