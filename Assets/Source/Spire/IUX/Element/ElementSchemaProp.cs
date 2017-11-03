using System;

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
}