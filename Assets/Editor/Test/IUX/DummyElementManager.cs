using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Test.UI
{
    public class DummyElementManager : IElementManager
    {
        private readonly List<Element> _elements = new List<Element>();

        public ReadOnlyCollection<Element> All { get; private set; }
        public Action<Element> OnCreated { get; set; }
        public Action<Element> OnDestroyed { get; set; }

        public void Add(Element element)
        {
            _elements.Add(element);
            if (OnCreated != null) OnCreated(element);
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

        public Element ByGuid(string guid)
        {
            return null;
        }
    }
}