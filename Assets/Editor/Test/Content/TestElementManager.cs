using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class TestElementManager : IElementManager
    {
        private List<Element> _elements = new List<Element>();
        public ReadOnlyCollection<Element> All
        {
            get { return _elements.AsReadOnly(); }
        }
        
        public Action<Element> OnCreated { get; set; }
        public Action<Element> OnDestroyed { get; set; }
        
        public void Add(Element element)
        {
            _elements.Add(element);
            
            if (OnCreated != null)
            {
                OnCreated(element);
            }
        }

        public Element ById(string id)
        {
            throw new NotImplementedException();
        }

        public Element ByGuid(string guid)
        {
            throw new NotImplementedException();
        }
    }
}