using System;

namespace CreateAR.EnkluPlayer
{
    public static class ActionExtensions
    {
        public static void Execute(this Action @this)
        {
            if (@this != null)
            {
                @this();
            }
        }

        public static void Execute<T>(this Action<T> @this, T arg)
        {
            if (@this != null)
            {
                @this(arg);
            }
        }
    }
}