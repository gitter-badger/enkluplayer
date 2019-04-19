using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using NUnit.Framework;

namespace CreateAR.EnkluPlayer.Test.UI
{
    [TestFixture]
    public class ElementQuery_Tests
    {
        private Element _element;

        private static readonly char[] _letters = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};

        [SetUp]
        public void Setup()
        {
            // generate data
            var description = new ElementDescription
            {
                Elements = GenerateElements().ToArray(),
                Root = GenerateRefs(0)
            };

            _element = Element_IntegrationTests.ElementFactory().Element(description);
        }

        private ElementRef GenerateRefs(int index)
        {
            var children = index == _letters.Length - 1
                ? new ElementRef[0]
                : new [] { GenerateRefs(index + 1) };

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
                        },
                        Bools = new Dictionary<string, bool>
                        {
                            { "Bar", i == 19 }
                        }
                    }
                });
            }

            return elements;
        }

        [Test]
        public void FindOneTrivial()
        {
            Assert.IsNull(_element.FindOne<Element>(null));
            Assert.IsNull(_element.FindOne<Element>(string.Empty));
        }

        [Test]
        public void FindOneShallow()
        {
            Assert.AreEqual("b", _element.FindOne<Element>("b").Id);
            Assert.IsNull(_element.FindOne<Element>("h"));
        }

        [Test]
        public void FindOneAbsPath()
        {
            Assert.AreEqual("z", _element.FindOne<Element>("b.c.d.e.f.g.h.i.j.k.l.m.n.o.p.q.r.s.t.u.v.w.x.y.z").Id);
            Assert.IsNull(_element.FindOne<Element>("a.b.d"));
        }

        [Test]
        public void FindOneRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne<Element>("b..f").Id);
            Assert.IsNull(_element.FindOne<Element>("b..b"));
        }

        [Test]
        public void FindOneStartRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne<Element>("..f").Id);
            Assert.IsNull(_element.FindOne<Element>("..boo"));
        }

        [Test]
        public void FindOneRecurPathTrivial()
        {
            Assert.AreEqual("c", _element.FindOne<Element>("b..c").Id);
        }

        [Test]
        public void FindOneAbsAndRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne<Element>("b.c..f").Id);
            Assert.IsNull(_element.FindOne<Element>("b..a"));
        }

        [Test]
        public void FindOneMultiRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne<Element>("b..c..f").Id);
            Assert.IsNull(_element.FindOne<Element>("b..c..a"));
        }

        [Test]
        public void FindOnePropString()
        {
            Assert.AreEqual("f", _element.FindOne<Element>("..(@Letter=f)").Id);
            Assert.IsNull(_element.FindOne<Element>("..(@Letter=zebra)"));
        }

        [Test]
        public void FindOnePropInt()
        {
            Assert.AreEqual("d", _element.FindOne<Element>("..(@Foo=3)").Id);
            Assert.IsNull(_element.FindOne<Element>("..(@Foo=34)"));
        }

        [Test]
        public void FindAll()
        {
            var elements = new List<Element>();
            _element.Find("*", elements);

            Assert.AreEqual(1, elements.Count);
            Assert.AreEqual("b", elements[0].Id);
        }

        [Test]
        public void FindAllDeeper()
        {
            var elements = new List<Element>();
            _element.Find("..d.*", elements);

            Assert.AreEqual(1, elements.Count);
            Assert.AreEqual("e", elements[0].Id);
        }

        [Test]
        public void FindAllRecursive()
        {
            var elements = new List<Element>();
            _element.Find("..*", elements);

            Assert.AreEqual(25, elements.Count);
        }

        [Test]
        public void FindAllRecursiveDeeper()
        {
            var elements = new List<Element>();
            _element.Find("..d..*", elements);

            Assert.AreEqual(22, elements.Count);
        }

        [Test]
        public void FindProp_Gt()
        {
            var elements = new List<Element>();
            _element.Find("..(@Foo>5)", elements);

            Assert.AreEqual(20, elements.Count);
        }

        [Test]
        public void FindProp_Gt_Equals()
        {
            var elements = new List<Element>();
            _element.Find("..(@Foo>=5)", elements);

            Assert.AreEqual(21, elements.Count);
        }

        [Test]
        public void FindProp_Lt()
        {
            var elements = new List<Element>();
            _element.Find("..(@Foo<5)", elements);

            Assert.AreEqual(4, elements.Count);
        }

        [Test]
        public void FindProp_Lt_Equals()
        {
            var elements = new List<Element>();
            _element.Find("..(@Foo<=5)", elements);

            Assert.AreEqual(5, elements.Count);
        }

        [Test]
        public void FindProp_Not_Equals()
        {
            var elements = new List<Element>();
            _element.Find("..(@Foo!=5)", elements);

            Assert.AreEqual(24, elements.Count);
        }

        [Test]
        public void FindProp_True()
        {
            var elements = new List<Element>();
            _element.Find("..(@Bar=true)", elements);

            Assert.AreEqual(1, elements.Count);
        }
    }
}