using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.Test
{
    public interface IElementFactory
    {
        Element Element(ElementDescription description);
    }

    public class ElementFactory : IElementFactory
    {
        public Element Element(ElementDescription description)
        {
            return Element(
                description.Root,
                description);
        }

        private Element Element(
            ElementRef reference,
            ElementDescription description)
        {
            // children first
            var referencedChildren = reference.Children;
            var children = new Element[referencedChildren.Length];
            for (int i = 0, len = referencedChildren.Length; i < len; i++)
            {
                children[i] = Element(
                    referencedChildren[i],
                    description);
            }

            // parent
            var id = reference.Id;
            var data = description.ById(id);
            var element = new Element();
            element.Load(data, children);

            return element;
        }
    }

    public class ElementDescription
    {
        public ElementRef Root;
        public ElementData[] Elements;

        public ElementData ById(string id)
        {
            var elements = Elements;
            for (int i = 0, len = elements.Length; i < len; i++)
            {
                var element = elements[i];
                if (element.Id == id)
                {
                    return element;
                }
            }

            return null;
        }
    }

    public class ElementRef
    {
        public string Id;
        public ElementRef[] Children = new ElementRef[0];
    }

    public class ElementData
    {
        public string Id;
        public ElementData[] Children = new ElementData[0];
    }

    public class Element
    {
        private readonly List<Element> _elements = new List<Element>();

        public string Guid { get; private set;  }
        public string Id { get; private set; }

        public Element[] Children
        {
            get
            {
                return _elements.ToArray();
            }
        }
        
        public void Load(ElementData data, Element[] children)
        {
            Guid = System.Guid.NewGuid().ToString();
            Id = data.Id;

            _elements.AddRange(children);

            LoadInternal();
        }

        public void Unload()
        {
            UnloadInternal();

            Id = string.Empty;

            _elements.Clear();
        }

        public void AddChild(Element child)
        {
            _elements.Add(child);
        }

        public bool RemoveChild(Element child)
        {
            throw new NotImplementedException();
        }

        protected virtual void LoadInternal()
        {
            
        }

        protected virtual void UnloadInternal()
        {

        }
    }
}