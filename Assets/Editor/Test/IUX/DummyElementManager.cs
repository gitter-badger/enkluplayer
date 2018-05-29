using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer.Test.UI
{
    public class DummyElementManager : IElementManager
    {
        private readonly List<Element> _elements = new List<Element>();

        public void Add(Element element)
        {
            _elements.Add(element);
        }

        public Element ById(string id)
        {
            for (int i = 0, len = _elements.Count; i < len; i++)
            {
                var element = _elements[i];
                if (element.Id == id)
                {
                    return element;
                }
            }

            return null;
        }
    }
}