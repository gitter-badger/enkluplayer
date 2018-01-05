using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.UI
{
    [TestFixture]
    public class Element_IntegrationTests
    {
        private const int FOO = 12;
        private const int FUZZ = 73;
        private const int BAR = 1443;

        private ElementFactory _factory;

        private readonly ElementDescription _data = new ElementDescription
        {
            Elements = new[]
            {
                new ElementData
                {
                    Id = "a",
                    Schema = new ElementSchemaData
                    {
                        Ints = new Dictionary<string, int>
                        {
                            {"foo", FOO}
                        }
                    }
                },
                new ElementData
                {
                    Id = "a.a",
                    Schema = new ElementSchemaData
                    {
                        Ints = new Dictionary<string, int>
                        {
                            {"fuzz", FUZZ}
                        }
                    }
                },
                new ElementData
                {
                    Id = "a.b"
                },
                new ElementData
                {
                    Id = "a.a.a"
                }
            },
            Root = new ElementRef
            {
                Id = "a",
                Children = new[]
                {
                    new ElementRef
                    {
                        Id = "a.a",
                        Children = new []
                        {
                            new ElementRef
                            {
                                Id = "a.a.a"
                            }
                        }
                    },
                    new ElementRef
                    {
                        Id = "a.b"
                    }
                }
            }
        };

        private readonly ElementDescription _newElement = new ElementDescription
        {
            Elements = new[]
            {
                new ElementData
                {
                    Id = "new"
                }
            },
            Root = new ElementRef
            {
                Id = "new",
                Schema = new ElementSchemaData
                {
                    Ints = new Dictionary<string, int>
                    {
                        { "bar", BAR }
                    }
                }
            }
        };

        [SetUp]
        public void Setup()
        {
            _factory = new ElementFactory(null, null, new DummyElementManager(), null, null, null, null, null, null);
        }

        [Test]
        public void SchemaGraphAddChildUpdate()
        {
            var element = _factory.Element(_data);
            var newElement = _factory.Element(_newElement);
            
            var aa = element.Children[0];
            aa.AddChild(newElement);
            
            Assert.AreEqual(FUZZ, newElement.Schema.Get<int>("fuzz").Value);
            Assert.AreEqual(FOO, newElement.Schema.Get<int>("foo").Value);
        }

        [Test]
        public void SchemaGraphRemoveChildUpdate()
        {
            var element = _factory.Element(_data);
            var newElement = _factory.Element(_newElement);

            var aa = element.Children[0];
            aa.AddChild(newElement);

            Assert.AreEqual(FUZZ, newElement.Schema.Get<int>("fuzz").Value);

            aa.RemoveChild(newElement);
            
            Assert.AreEqual(0, newElement.Schema.Get<int>("fuzz").Value);
        }
    }
}