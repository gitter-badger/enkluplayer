using System;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Extensions for Actions.
    /// </summary>
    public static class ActionExtensions
    {
        /// <summary>
        /// Invokes an Action if it is not null.
        /// </summary>
        public static void Execute(this Action @this)
        {
            if (@this != null)
            {
                @this();
            }
        }

        /// <summary>
        /// Invokes an Action if it is not null;
        /// </summary>
        public static void Execute<T>(this Action<T> @this, T arg)
        {
            if (@this != null)
            {
                @this(arg);
            }
        }

        /// <summary>
        /// Invokes an Action if it is not null;
        /// </summary>
        public static void Execute<T, K>(this Action<T, K> @this, T arg1, K arg2)
        {
            if (@this != null)
            {
                @this(arg1, arg2);
            }
        }
    }
}