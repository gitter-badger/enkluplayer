using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;
using NUnit.Framework;
using UnityEngine;

namespace CreateAR.SpirePlayer.Test.UI
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

            _element = new ElementFactory(null, null, null, null, null, null, null, null, null).Element(description);
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
            Assert.IsNull(_element.FindOne(null));
            Assert.IsNull(_element.FindOne(string.Empty));
        }

        [Test]
        public void FindOneShallow()
        {
            Assert.AreEqual("a", _element.FindOne("a").Id);
            Assert.IsNull(_element.FindOne("h"));
        }

        [Test]
        public void FindOneAbsPath()
        {
            Assert.AreEqual("z", _element.FindOne("a.b.c.d.e.f.g.h.i.j.k.l.m.n.o.p.q.r.s.t.u.v.w.x.y.z").Id);
            Assert.IsNull(_element.FindOne("a.b.d"));
        }

        [Test]
        public void FindOneRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne("a..f").Id);
            Assert.IsNull(_element.FindOne("a..a"));
        }

        [Test]
        public void FindOneStartRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne("..f").Id);
            Assert.IsNull(_element.FindOne("..boo"));
        }

        [Test]
        public void FindOneRecurPathTrivial()
        {
            Assert.AreEqual("b", _element.FindOne("a..b").Id);
        }

        [Test]
        public void FindOneAbsAndRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne("a.b..f").Id);
            Assert.IsNull(_element.FindOne("a.b..a"));
        }

        [Test]
        public void FindOneMultiRecurPath()
        {
            Assert.AreEqual("f", _element.FindOne("a..c..f").Id);
            Assert.IsNull(_element.FindOne("a..c..a"));
        }

        [Test]
        public void FindOnePropString()
        {
            Assert.AreEqual("f", _element.FindOne("..(@Letter=f)").Id);
            Assert.IsNull(_element.FindOne("..(@Letter=zebra)"));
        }

        [Test]
        public void FindOnePropInt()
        {
            Debug.Log(_element.ToTreeString());

            Assert.AreEqual("d", _element.FindOne("..(@Foo=3)").Id);
            Assert.IsNull(_element.FindOne("..(@Foo=34)"));
        }
    }
}