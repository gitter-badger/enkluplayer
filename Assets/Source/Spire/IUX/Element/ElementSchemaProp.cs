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
        /// The value of the prop.
        /// </summary>
        public T Value { get; internal set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Value.</param>
        internal ElementSchemaProp(T value)
            : base(typeof(T))
        {
            Value = value;
        }
    }
}