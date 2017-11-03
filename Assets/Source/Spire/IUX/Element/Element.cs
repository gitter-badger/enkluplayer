using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateAR.SpirePlayer.UI
{
    public class ElementStateProp
    {
        
    }

    public class ElementStateProp<T> : ElementStateProp
    {
        public T Value { get; private set; }
    }

    public class ElementState
    {
        private readonly List<string> _names = new List<string>();
        private readonly List<ElementStateProp> _props = new List<ElementStateProp>();

        public ElementState()
        {
            
        }

        public void Set<T>(string name, T value)
        {
            var index = _names.IndexOf(name);
            if (-1 == index)
            {

            }
            else
            {
                
            }
            throw new NotImplementedException();
        }

        public ElementStateProp<T> Get<T>(string name)
        {
            return _props[0] as ElementStateProp<T>;
            throw new NotImplementedException();
        }

        public void Wrap(ElementState state)
        {
            throw new NotImplementedException();
        }
    }

    public class ElementRef
    {
        public string Id;
        public ElementRef[] Children = new ElementRef[0];

        public override string ToString()
        {
            return string.Format("[ElementRef Id={0}, ChildCount={1}]",
                Id,
                Children.Length);
        }
    }

    public class ElementData
    {
        public string Id;
        public ElementData[] Children = new ElementData[0];

        public ElementData()
        {
            
        }

        public ElementData(ElementData data)
        {
            Id = data.Id;
            Children = data.Children.ToArray();
        }
    }

    public class Element
    {
        private readonly List<Element> _children = new List<Element>();

        public string Guid { get; private set;  }
        public string Id { get; private set; }

        public Element[] Children
        {
            get
            {
                return _children.ToArray();
            }
        }
        
        internal void Load(ElementData data, Element[] children)
        {
            Guid = System.Guid.NewGuid().ToString();
            Id = data.Id;

            _children.AddRange(children);

            LoadInternal();
        }

        internal void Unload()
        {
            UnloadInternal();

            Id = string.Empty;

            _children.Clear();
        }

        public void AddChild(Element child)
        {
            throw new NotImplementedException();
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