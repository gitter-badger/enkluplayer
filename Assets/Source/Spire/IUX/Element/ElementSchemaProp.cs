using System;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Base class used to track type.
    /// </summary>
    public abstract class ElementSchemaProp
    {
        /// <summary>
        /// Name of the prop. Can be null for shared, default value props.
        /// </summary>
        internal readonly string Name;

        /// <summary>
        /// The type of the prop.
        /// </summary>
        internal readonly Type Type;

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
        /// True iff the link has been broken between parent and child.
        /// </summary>
        private bool _linkBroken;

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

                    _linkBroken = true;
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
        /// Constructor.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        internal ElementSchemaProp(string name, T value)
            : base(name, typeof(T))
        {
            _value = value;
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
            if (_linkBroken)
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