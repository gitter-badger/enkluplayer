using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test.UI
{
    [TestFixture]
    public class ElementFactory_Tests
    {
        private ElementFactory _elements;

        private static readonly char[] _letters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        private ElementRef GenerateRefs(int index)
        {
            var children = index == _letters.Length - 1
                ? new ElementRef[0]
                : new[] { GenerateRefs(index + 1) };

            return new ElementRef
            {
                Id = _letters[index].ToString(),
                Children = children
            };
        }

        private List<ElementData> GenerateElements()
        {
            var elements = new List<ElementData>();
            for (int i = 0, len = _letters.Length; i < len; i++)
            {
                var id = _letters[i].ToString();
                elements.Add(new ElementData
                {
                    Id = id,
                    Schema = new ElementSchemaData
                    {
                        Ints = new Dictionary<string, int>
                        {
                            { "Foo", i }
                        },
                        Strings = new Dictionary<string, string>
                        {
                            { "Letter", id }
                        }
                    }
                });
            }

            return elements;
        }

        [SetUp]
        public void Setup()
        {
            _elements = new ElementFactory(
                new DummyPrimitiveFactory(), 
                null, null,
                new DummyElementManager(),
                null, null, null,
                new MessageRouter(),
                null, null, null, null, null, null, null);
        }

        [Test]
        public void CreateElement()
        {
            var id = Guid.NewGuid().ToString();
            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = id
                },
                Elements = new []
                {
                    new ElementData
                    {
                        Id = id
                    }
                }
            });

            Assert.AreEqual(id, element.Id);
        }

        [Test]
        public void CreateElementRefChildren()
        {
            var a = Guid.NewGuid().ToString();
            var b = Guid.NewGuid().ToString();
            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = a,
                    Children = new []
                    {
                        new ElementRef
                        {
                            Id = b
                        }
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = a
                    },
                    new ElementData
                    {
                        Id = b
                    },
                }
            });

            Assert.AreEqual(a, element.Id);
            Assert.AreEqual(b, element.Children[0].Id);
        }

        [Test]
        public void CreateElementManyRefChildren()
        {
            var a = Guid.NewGuid().ToString();
            var b = Guid.NewGuid().ToString();
            var c = Guid.NewGuid().ToString();
            var d = Guid.NewGuid().ToString();
            var e = Guid.NewGuid().ToString();

            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = a,
                    Children = new[]
                    {
                        new ElementRef
                        {
                            Id = b
                        },
                        new ElementRef
                        {
                            Id = c,
                            Children = new []
                            {
                                new ElementRef
                                {
                                    Id = d
                                }
                            }
                        },
                        new ElementRef
                        {
                            Id = e
                        }
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = a
                    },
                    new ElementData
                    {
                        Id = b
                    },
                    new ElementData
                    {
                        Id = c
                    },
                    new ElementData
                    {
                        Id = d
                    },
                    new ElementData
                    {
                        Id = e
                    }
                }
            });

            Assert.AreEqual(a, element.Id);
            Assert.AreEqual(b, element.Children[0].Id);
            Assert.AreEqual(c, element.Children[1].Id);
            Assert.AreEqual(d, element.Children[1].Children[0].Id);
            Assert.AreEqual(e, element.Children[2].Id);
        }

        [Test]
        public void CreateElementChildren()
        {
            var a = Guid.NewGuid().ToString();
            var b = Guid.NewGuid().ToString();
            var c = Guid.NewGuid().ToString();
            var d = Guid.NewGuid().ToString();

            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = a,
                    Children = new[]
                    {
                        new ElementRef
                        {
                            Id = b
                        }
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = a,
                        Children = new []
                        {
                            new ElementData
                            {
                                Id = c
                            }
                        }
                    },
                    new ElementData
                    {
                        Id = b,
                        Children = new []
                        {
                            new ElementData
                            {
                                Id = d
                            }
                        }
                    }
                }
            });

            Assert.AreEqual(a, element.Id);
            Assert.AreEqual(c, element.Children[0].Id);
            Assert.AreEqual(b, element.Children[1].Id);
            Assert.AreEqual(d, element.Children[1].Children[0].Id);
        }

        [Test]
        public void CreateElementNoData()
        {
            var a = Guid.NewGuid().ToString();
            Assert.Throws<Exception>(() =>
            {
                _elements.Element(new ElementDescription
                {
                    Root = new ElementRef
                    {
                        Id = "foo",
                        Children = new[]
                        {
                            new ElementRef
                            {
                                Id = a
                            }
                        }
                    },
                    Elements = new[]
                    {
                        new ElementData
                        {
                            Id = "bar"
                        }
                    }
                });
            });
        }

        [Test]
        public void CreateElementMultiData()
        {
            var a = Guid.NewGuid().ToString();
            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = a,
                    Children = new[]
                    {
                        new ElementRef
                        {
                            Id = a
                        }
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = a,
                        Children = new []
                        {
                            new ElementData
                            {
                                Id = "foo"
                            }
                        }
                    },
                    new ElementData
                    {
                        Id = a
                    }
                }
            });

            Assert.AreSame(a, element.Id);
            Assert.AreSame("foo", element.Children[0].Id);
        }

        [Test]
        public void CreateElementWithSchema()
        {
            var id = Guid.NewGuid().ToString();
            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = id
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = id,
                        Schema = new ElementSchemaData
                        {
                            Ints = new Dictionary<string, int>
                            {
                                { "foo", 5 }
                            }
                        }
                    }
                 }
            });

            Assert.AreEqual(5, element.Schema.Get<int>("foo").Value);
        }

        [Test]
        public void CreateElementWithRefSchema()
        {
            var id = Guid.NewGuid().ToString();
            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = id,
                    Schema = new ElementSchemaData
                    {
                        Ints = new Dictionary<string, int>
                        {
                            { "foo", 5 }
                        }
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = id
                    }
                }
            });

            Assert.AreEqual(5, element.Schema.Get<int>("foo").Value);
        }

        [Test]
        public void CreateElementWithRefOverwriteSchema()
        {
            var id = Guid.NewGuid().ToString();
            var element = _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = id,
                    Schema = new ElementSchemaData
                    {
                        Ints = new Dictionary<string, int>
                        {
                            { "foo", 6 }
                        }
                    }
                },
                Elements = new[]
                {
                    new ElementData
                    {
                        Id = id,
                        Schema = new ElementSchemaData
                        {
                            Ints = new Dictionary<string, int>
                            {
                                { "foo", 5 }
                            }
                        }
                    }
                }
            });

            Assert.AreEqual(6, element.Schema.Get<int>("foo").Value);
        }

        [Test]
        public void GenerateAZ()
        {
            var description = new ElementDescription
            {
                Elements = GenerateElements().ToArray(),
                Root = GenerateRefs(0)
            };

            var element = _elements.Element(description);

            Assert.AreEqual("a", element.Id);
            Assert.AreEqual("c", element.Children[0].Children[0].Id);

            Assert.AreEqual(
                "c",
                element.Children[0].Children[0].Schema.Get<string>("Letter").Value);
        }
    }
}