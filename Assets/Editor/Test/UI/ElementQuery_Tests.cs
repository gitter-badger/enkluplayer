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

            _element = new ElementFactory().Element(description);
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
                elements.Add(new ElementData
                {
                    Id = _letters[i].ToString()
                });
            }

            return elements;
        }

        [Test]
        public void SearchBase()
        {
           Debug.Log(_element.ToTreeString());
        }
    }
}