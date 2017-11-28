using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.UI;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test.UI
{
    [TestFixture]
    public class ElementQuery_Tests
    {
        private IElement _element;

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

            _element = new ElementFactory(
                new DummyPrimitiveFactory(),
                null,
                new DummyElementManager(),
                null, null, null, null,
                new MessageRouter(),
                null).Element(description);
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
                        }
                    }
                });
            }

            return elements;
        }

        [Test]
        public void FindOneTrivial()
        {
            Assert.IsNull(_element.FindOne<IElement>(null));
            Assert.IsNull(_element.FindOne<IElement>(string.Empty));
        }

        [Test]
        public void FindOneShallow()
        {
            Debug.Log(_element.ToTreeString());
            Assert.AreEqual("b", _element.FindOne<IElement>("b").Id);
            Assert.IsNull(_element.FindOne<IElement>("h"));
        }

        [Test]
        public void FindOneAbsPath()
        {
            Assert.AreEqual("z", _element.FindOne<IElement>("b.c.d.e.f.g.h.i.j.k.l.m.n.o.p.q.r.s.t.u.v.w.x.y.z").Id);
            Assert.IsNull(_element.FindOne<IElement>("a.b.d"));
        }

        [Test]
        public void FindOneRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne<IElement>("b..f").Id);
            Assert.IsNull(_element.FindOne<IElement>("b..b"));
        }

        [Test]
        public void FindOneStartRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne<IElement>("..f").Id);
            Assert.IsNull(_element.FindOne<IElement>("..boo"));
        }

        [Test]
        public void FindOneRecurPathTrivial()
        {
            Assert.AreEqual("c", _element.FindOne<IElement>("b..c").Id);
        }

        [Test]
        public void FindOneAbsAndRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne<IElement>("b.c..f").Id);
            Assert.IsNull(_element.FindOne<IElement>("b..a"));
        }

        [Test]
        public void FindOneMultiRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne<IElement>("b..c..f").Id);
            Assert.IsNull(_element.FindOne<IElement>("b..c..a"));
        }

        [Test]
        public void FindOnePropString()
        {
            Assert.AreEqual("f", _element.FindOne<IElement>("..(@Letter=f)").Id);
            Assert.IsNull(_element.FindOne<IElement>("..(@Letter=zebra)"));
        }

        [Test]
        public void FindOnePropInt()
        {
            Assert.AreEqual("d", _element.FindOne<IElement>("..(@Foo=3)").Id);
            Assert.IsNull(_element.FindOne<IElement>("..(@Foo=34)"));
        }

        [Test]
        public void FindAll()
        {
            var elements = new List<IElement>();
            _element.Find("*", elements);

            Assert.AreEqual(1, elements.Count);
            Assert.AreEqual("b", elements[0].Id);
        }
        
        [Test]
        public void FindAllDeeper()
        {
            var elements = new List<IElement>();
            _element.Find("..d.*", elements);

            Assert.AreEqual(1, elements.Count);
            Assert.AreEqual("e", elements[0].Id);
        }

        [Test]
        public void FindAllRecursive()
        {
            var elements = new List<IElement>();
            _element.Find("..*", elements);

            Assert.AreEqual(25, elements.Count);
        }

        [Test]
        public void FindAllRecursiveDeeper()
        {
            var elements = new List<IElement>();
            _element.Find("..d..*", elements);

            Assert.AreEqual(22, elements.Count);
        }
    }
}