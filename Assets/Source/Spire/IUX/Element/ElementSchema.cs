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
        /// List of all props.
        /// </summary>
        private readonly List<ElementSchemaProp> _props = new List<ElementSchemaProp>();

        /// <summary>
        /// List of default value props.
        /// </summary>
        private readonly List<ElementSchemaProp> _defaultValueProps = new List<ElementSchemaProp>();

        /// <summary>
        /// Parent schema.
        /// </summary>
        private ElementSchema _parent;

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
                    _props.Add(new ElementSchemaProp<int>(
                        prop.Key,
                        prop.Value));
                }
            }

            if (null != data.Floats)
            {
                foreach (var prop in data.Floats)
                {
                    _props.Add(new ElementSchemaProp<float>(
                        prop.Key,
                        prop.Value));
                }
            }

            if (null != data.Bools)
            {
                foreach (var prop in data.Bools)
                {
                    _props.Add(new ElementSchemaProp<bool>(
                        prop.Key,
                        prop.Value));
                }
            }

            if (null != data.Strings)
            {
                foreach (var prop in data.Strings)
                {
                    _props.Add(new ElementSchemaProp<string>(
                        prop.Key,
                        prop.Value));
                }
            }

            if (null != data.Vectors)
            {
                foreach (var prop in data.Vectors)
                {
                    _props.Add(new ElementSchemaProp<Vec3>(
                        prop.Key,
                        prop.Value));
                }
            }
        }

        /// <summary>
        /// Wraps around a schema, using the provided schema as a fallback.
        /// </summary>
        /// <param name="schema">The schema to wrap.</param>
        public void Wrap(ElementSchema schema)
        {
            _parent = schema;

            var parentProps = schema._props;
            for (int i = 0, ilen = _props.Count; i < ilen; i++)
            {
                var prop = _props[i];
                var name = prop.Name;
                var type = prop.Type;

                for (int j = 0, jlen = parentProps.Count; j < jlen; j++)
                {
                    var parentProp = parentProps[j];
                    if (parentProp.Name == name && parentProp.Type == type)
                    {
                        prop.Reparent(parentProp);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets a value.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value.</param>
        public void Set<T>(string name, T value)
        {
            var prop = Prop(name);
            if (null == prop)
            {
                _props.Add(new ElementSchemaProp<T>(name, value));
            }
            else
            {
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
            var type = typeof(T);
            var prop = Prop(name);
            if (null == prop)
            {
                // check parent
                if (null == _parent)
                {
                    prop = new ElementSchemaProp<T>(name, default(T));
                }
                else
                {
                    prop = new ElementSchemaProp<T>(name, _parent.Get<T>(name));
                }

                if (type == prop.Type)
                {
                    _props.Add(prop);
                }
            }

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
            for (int i = 0, len = _defaultValueProps.Count; i < len; i++)
            {
                var prop = _defaultValueProps[i];
                if (type == prop.Type)
                {
                    return (ElementSchemaProp<T>) _defaultValueProps[i];
                }
            }

            var defaultProp = new ElementSchemaProp<T>(
                string.Empty,
                default(T));
            _defaultValueProps.Add(defaultProp);

            return defaultProp;
        }

        /// <summary>
        /// Retrieves the prop with matching name. Does not return a default
        /// value prop.
        /// </summary>
        /// <param name="name">Name of the prop.</param>
        /// <returns></returns>
        private ElementSchemaProp Prop(string name)
        {
            for (int i = 0, len = _props.Count; i < len; i++)
            {
                var prop = _props[i];
                if (prop.Name == name)
                {
                    return prop;
                }
            }

            return null;
        }
    }
}