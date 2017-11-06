using System.Collections.Generic;
using System.Text;
using CreateAR.SpirePlayer.UI;
using NUnit.Framework;
using UnityEngine;

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
            _factory = new ElementFactory();
        }

        [Test]
        public void SchemaGraphUpdate()
        {
            var element = _factory.Element(_data);
            var newElement = _factory.Element(_newElement);
            
            var aa = element.Children[0];
            aa.AddChild(newElement);

            Log(element);

            Assert.AreEqual(FUZZ, newElement.Schema.Get<int>("fuzz").Value);
            Assert.AreEqual(FOO, newElement.Schema.Get<int>("foo").Value);
        }

        private void Log(Element element)
        {
            var builder = new StringBuilder();
            Append(builder, element);

            Debug.Log(builder);
        }

        private void Append(StringBuilder builder, Element element, int tabs = 0)
        {
            for (var i = 0; i < tabs; i++)
            {
                builder.Append("\t");
            }

            builder.AppendFormat("{0}\n", element);

            var children = element.Children;
            for (int i = 0, len = children.Length; i < len; i++)
            {
                Append(builder, children[i], tabs + 1);
            }
        }
    }
}