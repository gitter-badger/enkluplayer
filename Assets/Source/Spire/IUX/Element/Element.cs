using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.UI
{
    public class Element
    {
        private readonly List<Element> _children = new List<Element>();

        public string Guid { get; private set;  }
        public string Id { get; private set; }
        public ElementSchema Schema { get; private set; }

        public Element[] Children
        {
            get
            {
                return _children.ToArray();
            }
        }
        
        internal void Load(
            ElementData data,
            ElementSchema schema,
            Element[] children)
        {
            Guid = System.Guid.NewGuid().ToString();
            Id = data.Id;
            Schema = schema;

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