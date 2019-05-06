using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Enklu.Data;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Composable state object.
    /// </summary>
    public class ElementSchema : IEnumerable<ElementSchemaProp>
    {
        /// <summary>
        /// Internal enumerator for schema.
        /// </summary>
        public class SchemaEnumerator : IEnumerator<ElementSchemaProp>
        {
            /// <summary>
            /// Props to enumerate.
            /// </summary>
            private readonly List<ElementSchemaProp> _props;

            /// <summary>
            /// Original length of props.
            /// </summary>
            private readonly int _propLen;

            /// <summary>
            /// Index into props.
            /// </summary>
            private int _propIndex = -1;

            /// <inheritdoc cref="IEnumerator"/>
            public ElementSchemaProp Current { get; private set; }

            /// <inheritdoc cref="IEnumerator"/>
            object IEnumerator.Current
            {
                get { return Current; }
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="schema">The schema to iterate over.</param>
            public SchemaEnumerator(ElementSchema schema)
            {
                _props = schema._props;
                _propLen = _props.Count;

                Reset();
            }

            /// <inheritdoc cref="IEnumerator"/>
            public void Dispose()
            {
                //
            }

            /// <inheritdoc cref="IEnumerator"/>
            public bool MoveNext()
            {
                // bounds check against original length to act like a real life
                // interator
                while (++_propIndex < _propLen)
                {
                    var next = _props[_propIndex];
                    if (next.LinkBroken)
                    {
                        Current = next;
                        return true;
                    }
                }

                return false;
            }

            /// <inheritdoc cref="IEnumerator"/>
            public void Reset()
            {
                _propIndex = -1;
            }
        }

        /// <summary>
        /// List of all props.
        /// </summary>
        private readonly List<ElementSchemaProp> _props = new List<ElementSchemaProp>();

        /// <summary>
        /// List of default value props.
        /// </summary>
        private readonly List<ElementSchemaProp> _defaultValueProps = new List<ElementSchemaProp>();

        /// <summary>
        /// Retrieves the parent schema.
        /// </summary>
        public ElementSchema Parent { get; private set; }

        /// <summary>
        /// Identifier.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Called when a new property has been added to self.
        /// </summary>
        public event Action<string, Type> OnSelfPropAdded;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementSchema()
        {
            //
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        public ElementSchema(string identifier)
        {
            Identifier = identifier;
        }

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("{");
            for (int i = 0, len = _props.Count; i < len; i++)
            {
                var prop = _props[i];
                builder.Append(string.Format(" {0}={1} ",
                    prop.Name,
                    prop));
            }
            builder.Append("}");

            return builder.ToString();
        }

        /// <inheritdoc cref="IEnumerable"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            if (null != Parent)
            {
                yield return Parent.GetEnumerator();
            }

            for (int i = 0, len = _props.Count; i < len; i++)
            {
                var prop = _props[i];
                if (!prop.LinkBroken)
                {
                    continue;
                }

                yield return prop;
            }
        }

        /// <inheritdoc cref="IEnumerable"/>
        public IEnumerator<ElementSchemaProp> GetEnumerator()
        {
            return new SchemaEnumerator(this);
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
                    _props.Add(new ElementSchemaProp<int>(
                        prop.Key,
                        prop.Value,
                        false));
                }
            }

            if (null != data.Floats)
            {
                foreach (var prop in data.Floats)
                {
                    _props.Add(new ElementSchemaProp<float>(
                        prop.Key,
                        prop.Value,
                        false));
                }
            }

            if (null != data.Bools)
            {
                foreach (var prop in data.Bools)
                {
                    _props.Add(new ElementSchemaProp<bool>(
                        prop.Key,
                        prop.Value,
                        false));
                }
            }

            if (null != data.Strings)
            {
                foreach (var prop in data.Strings)
                {
                    _props.Add(new ElementSchemaProp<string>(
                        prop.Key,
                        prop.Value,
                        false));
                }
            }

            if (null != data.Vectors)
            {
                foreach (var prop in data.Vectors)
                {
                    _props.Add(new ElementSchemaProp<Vec3>(
                        prop.Key,
                        prop.Value,
                        false));
                }
            }

            if (null != data.Colors)
            {
                foreach (var prop in data.Colors)
                {
                    _props.Add(new ElementSchemaProp<Col4>(
                        prop.Key,
                        prop.Value,
                        false));
                }
            }
        }

        /// <summary>
        /// Wraps around a schema, using the provided schema as a fallback.
        /// </summary>
        /// <param name="schema">The schema to wrap.</param>
        public void Wrap(ElementSchema schema)
        {
            // detect cycles
            var node = schema;
            while (null != node)
            {
                if (node == this)
                {
                    throw new Exception("Schema cannot wrap self.");
                }

                node = node.Parent;
            }

            Parent = schema;

            if (null == schema)
            {
                for (int i = 0, ilen = _props.Count; i < ilen; i++)
                {
                    _props[i].Reparent(null);
                }

                return;
            }

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
        /// Inherits properties from a schema. Copies values so that changes
        /// to one do not affect the other.
        /// </summary>
        /// <param name="schema">The schema to inherit from.</param>
        public void Inherit(ElementSchema schema)
        {
            while (null != schema)
            {
                // copy props
                foreach (var prop in schema)
                {
                    var existing = Prop(prop.Name);
                    if (null != existing)
                    {
                        continue;
                    }

                    _props.Add(prop.Copy());
                }

                schema = schema.Parent;
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
                _props.Add(new ElementSchemaProp<T>(name, value, false));

                if (null != OnSelfPropAdded)
                {
                    OnSelfPropAdded(name, typeof(T));
                }
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
                if (null == Parent)
                {
                    prop = new ElementSchemaProp<T>(name, default(T), true);
                }
                else
                {
                    prop = new ElementSchemaProp<T>(name, Parent.Get<T>(name));
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
        /// Returns the a prop if locally defined, or adds the default.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public ElementSchemaProp<T> GetOwn<T>(string name, T @default)
        {
            var type = typeof(T);
            var prop = Prop(name);
            if (null == prop)
            {
                // add the default
                prop = new ElementSchemaProp<T>(name, @default, false);
                _props.Add(prop);

                if (null != OnSelfPropAdded)
                {
                    OnSelfPropAdded(name, typeof(T));
                }

                return (ElementSchemaProp<T>) prop;
            }

            if (type == prop.Type)
            {
                return (ElementSchemaProp<T>) prop;
            }

            return Default<T>();
        }
        
        /// <summary>
        /// Returns true iff the schema or parent schemas have a property with
        /// matching name.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <returns></returns>
        public bool HasProp(string name)
        {
            if (HasOwnProp(name))
            {
                return true;
            }

            var parent = Parent;
            while (null != parent)
            {
                if (parent.HasOwnProp(name))
                {
                    return true;
                }

                parent = parent.Parent;
            }

            return false;
        }

        /// <summary>
        /// Returns true iff this object has a non-linked prop with matching name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasOwnProp(string name)
        {
            var prop = Prop(name);
            if (null == prop)
            {
                return false;
            }

            if (prop.LinkBroken)
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Returns a ReadOnlyCollection of all props specific to this Element.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ReadOnlyCollection<ElementSchemaProp> GetOwnProps()
        {
            return _props.AsReadOnly();
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
                default(T),
                false);
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