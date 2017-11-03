using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.UI
{
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
}