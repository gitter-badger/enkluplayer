using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateAR.SpirePlayer.UI
{
    public class ElementSchemaProp
    {
        internal readonly Type Type;

        public ElementSchemaProp(Type type)
        {
            Type = type;
        }
    }

    public class ElementSchemaProp<T> : ElementSchemaProp
    {
        public T Value { get; internal set; }

        public ElementSchemaProp(T value)
            : base(typeof(T))
        {
            Value = value;
        }
    }
    
    public class ElementSchema
    {
        private readonly List<string> _names = new List<string>();
        private readonly List<ElementSchemaProp> _props = new List<ElementSchemaProp>();

        private readonly List<Type> _defaultValueTypes = new List<Type>();
        private readonly List<ElementSchemaProp> _defaultValueProps = new List<ElementSchemaProp>();

        private ElementSchema _parent;

        public ElementSchema()
        {
            //
        }

        public void Set<T>(string name, T value)
        {
            var index = _names.IndexOf(name);
            if (-1 == index)
            {
                var prop = new ElementSchemaProp<T>(value);
                _names.Add(name);
                _props.Add(prop);
            }
            else
            {
                var prop = _props[index];
                if (typeof(T) == prop.Type)
                {
                    ((ElementSchemaProp<T>) prop).Value = value;
                }

                // disregard non-matching types
            }
        }

        public ElementSchemaProp<T> Get<T>(string name)
        {
            ElementSchemaProp prop;
            var index = _names.IndexOf(name);
            if (-1 == index)
            {
                // get value from parent
                var value = null == _parent
                    ? default(T)
                    : _parent.Get<T>(name).Value;
                prop = new ElementSchemaProp<T>(value);

                _names.Add(name);
                _props.Add(prop);
            }
            else
            {
                prop = _props[index];
            }

            var type = typeof(T);
            if (type == prop.Type)
            {
                return (ElementSchemaProp<T>) prop;
            }

            return Default<T>();
        }

        public void Wrap(ElementSchema schema)
        {
            if (null != _parent)
            {
                throw new ArgumentException("Cannot wrap more than one schema.");
            }

            _parent = schema;
        }

        private ElementSchemaProp<T> Default<T>()
        {
            var type = typeof(T);
            for (int i = 0, len = _defaultValueTypes.Count; i < len; i++)
            {
                if (type == _defaultValueTypes[i])
                {
                    return (ElementSchemaProp<T>) _defaultValueProps[i];
                }
            }

            var prop = new ElementSchemaProp<T>(default(T));
            _defaultValueTypes.Add(type);
            _defaultValueProps.Add(prop);

            return prop;
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