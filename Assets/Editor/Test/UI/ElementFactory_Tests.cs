using System;
using CreateAR.SpirePlayer.UI;
using NUnit.Framework;

namespace CreateAR.SpirePlayer.Test
{
    public class ElementState_Tests
    {
        
    }

    [TestFixture]
    public class ElementFactory_Tests
    {
        private ElementFactory _elements;

        [SetUp]
        public void Setup()
        {
            _elements = new ElementFactory();
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
    }
}