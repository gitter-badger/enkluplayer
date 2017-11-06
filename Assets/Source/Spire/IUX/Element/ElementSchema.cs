using System;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Composable state object.
    /// </summary>
    public class ElementSchema
    {
        /// <summary>
        /// Parallel arrays that keeps name + prop aligned.
        /// </summary>
        private readonly List<string> _names = new List<string>();
        private readonly List<ElementSchemaProp> _props = new List<ElementSchemaProp>();

        /// <summary>
        /// Parallel arrays that keep name + static, default value props aligned.
        /// </summary>
        private readonly List<Type> _defaultValueTypes = new List<Type>();
        private readonly List<ElementSchemaProp> _defaultValueProps = new List<ElementSchemaProp>();

        /// <summary>
        /// Parent schema.
        /// </summary>
        private ElementSchema _parent;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ElementSchema()
        {
            //
        }

        /// <summary>
        /// Loads data, which contains all properties.
        /// </summary>
        /// <param name="data">Data object.</param>
        public void Load(ElementSchemaData data)
        {
            if (null != data.Ints)
            {
                foreach (var prop in data.Ints)
                {
                    _names.Add(prop.Key);
                    _props.Add(new ElementSchemaProp<int>(prop.Value));
                }
            }

            if (null != data.Floats)
            {
                foreach (var prop in data.Floats)
                {
                    _names.Add(prop.Key);
                    _props.Add(new ElementSchemaProp<float>(prop.Value));
                }
            }

            if (null != data.Bools)
            {
                foreach (var prop in data.Bools)
                {
                    _names.Add(prop.Key);
                    _props.Add(new ElementSchemaProp<bool>(prop.Value));
                }
            }

            if (null != data.Strings)
            {
                foreach (var prop in data.Strings)
                {
                    _names.Add(prop.Key);
                    _props.Add(new ElementSchemaProp<string>(prop.Value));
                }
            }

            if (null != data.Vectors)
            {
                foreach (var prop in data.Vectors)
                {
                    _names.Add(prop.Key);
                    _props.Add(new ElementSchemaProp<Vec3>(prop.Value));
                }
            }
        }

        /// <summary>
        /// Wraps around a schema, using the provided schema as a fallback.
        /// </summary>
        /// <param name="schema">The schema to wrap.</param>
        public void Wrap(ElementSchema schema)
        {
            if (null != _parent)
            {
                throw new ArgumentException("Cannot wrap more than one schema.");
            }

            _parent = schema;
        }

        /// <summary>
        /// Sets a value.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value.</param>
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

        /// <summary>
        /// Retrieves a value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <returns></returns>
        public ElementSchemaProp<T> Get<T>(string name)
        {
            ElementSchemaProp prop;
            var index = _names.IndexOf(name);
            if (-1 == index)
            {
                // check parent
                if (null == _parent)
                {
                    prop = new ElementSchemaProp<T>(default(T));
                }
                else
                {
                    prop = new ElementSchemaProp<T>(_parent.Get<T>(name));
                }

                _names.Add(name);
                _props.Add(prop);
            }
            else
            {
                prop = _props[index];
            }

            // TODO: Memory leak: prop may be added to list but not used.
            var type = typeof(T);
            if (type == prop.Type)
            {
                return (ElementSchemaProp<T>) prop;
            }

            return Default<T>();
        }

        /// <summary>
        /// Retrieves a property with a default value.
        /// </summary>
        /// <typeparam name="T">The type to return a prop for.</typeparam>
        /// <returns></returns>
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