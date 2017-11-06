using System;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Base class used to track type.
    /// </summary>
    public class ElementSchemaProp
    {
        /// <summary>
        /// The type of the prop.
        /// </summary>
        internal readonly Type Type;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="type">The type of prop.</param>
        internal ElementSchemaProp(Type type)
        {
            Type = type;
        }
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
        /// <param name="value">Value.</param>
        internal ElementSchemaProp(T value)
            : base(typeof(T))
        {
            _value = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parent">Parent prop, if any.</param>
        internal ElementSchemaProp(ElementSchemaProp<T> parent)
            : base(typeof(T))
        {
            _parent = parent;
            _value = _parent.Value;

            if (null != _parent)
            {
                _parent.OnChanged += Parent_OnChanged;
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