using System;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Base class used to track type.
    /// </summary>
    public abstract class ElementSchemaProp
    {
        /// <summary>
        /// Name of the prop. Can be null for shared, default value props.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The type of the prop.
        /// </summary>
        public readonly Type Type;
        
        /// <summary>
        /// True iff the link has been broken between parent and child.
        /// </summary>
        public bool LinkBroken;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="name">The name of the prop.</param>
        /// <param name="type">The type of prop.</param>
        internal ElementSchemaProp(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Reparents the prop.
        /// </summary>
        /// <param name="parent">The new parent.</param>
        internal abstract void Reparent(ElementSchemaProp parent);

        /// <summary>
        /// Copies prop.
        /// </summary>
        /// <returns>New prop.</returns>
        internal abstract ElementSchemaProp Copy();
    }

    /// <summary>
    /// Object that holds the value of a schema prop.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ElementSchemaProp<T> : ElementSchemaProp
    {
        /// <summary>
        /// Parent prop.
        /// </summary>
        private ElementSchemaProp<T> _parent;

        /// <summary>
        /// Backing variable for <c>Value</c> property.
        /// </summary>
        private T _value;

        /// <summary>
        /// The value of the prop.
        /// </summary>
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                // break connection
                if (null != _parent)
                {
                    _parent.OnChanged -= Parent_OnChanged;
                    _parent = null;

                    LinkBroken = true;
                }

                // return if there's no change in the value
                //  so no listeners get invoked.
                if (_value == null && value == null)
                {
                    return;
                }

                if (_value != null && _value.Equals(value))
                {
                    return;
                }

                var prev = _value;
                _value = value;

                if (null != OnChanged)
                {
                    OnChanged(this, prev, _value);
                }
            }
        }
        
        /// <summary>
        /// Called when the element has been changed.
        /// </summary>
        public event Action<ElementSchemaProp<T>, T, T> OnChanged;

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[Prop Name={0}, Value={1}]",
                Name,
                _value);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        /// <param name="allowInherit">If true, allows inherit from parents.</param>
        internal ElementSchemaProp(string name, T value, bool allowInherit)
            : base(name, typeof(T))
        {
            _value = value;
            LinkBroken = !allowInherit;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="parent">Parent prop, if any.</param>
        internal ElementSchemaProp(
            string name,
            ElementSchemaProp<T> parent)
            : base(name, typeof(T))
        {
            Reparent(parent);
        }

        /// <inheritdoc cref="ElementSchemaProp"/>
        internal sealed override void Reparent(ElementSchemaProp parent)
        {
            // the parent-link has been broken
            if (LinkBroken)
            {
                return;
            }

            // parent link is alive! swap parent!
            if (null != _parent)
            {
                _parent.OnChanged -= Parent_OnChanged;
            }

            _parent = (ElementSchemaProp<T>) parent;

            if (null != _parent)
            {
                _value = _parent.Value;
                _parent.OnChanged += Parent_OnChanged;
            }
            // when set to null
            else
            {
                _value = default(T);
            }
        }

        /// <inheritdoc />
        internal override ElementSchemaProp Copy()
        {
            return new ElementSchemaProp<T>(Name, Value, false);
        }

        /// <summary>
        /// Called when the parent value has changed.
        /// </summary>
        /// <param name="parent">Parent prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Parent_OnChanged(
            ElementSchemaProp<T> parent,
            T prev,
            T next)
        {
            _value = next;

            if (null != OnChanged)
            {
                OnChanged(this, prev, next);
            }
        }
    }
}