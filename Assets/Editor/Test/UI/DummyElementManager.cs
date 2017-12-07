using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer.Test.UI
{
    public class DummyElementManager : IElementManager
    {
        private readonly List<Element> _elements = new List<Element>();

        public void Add(Element element)
        {
            _elements.Add(element);
        }
    }
}