using System;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// String.Format.
        /// </summary>
        public static string Format(this string @this, params object[] replacements)
        {
            return string.Format(@this, replacements);
        }

        /// <summary>
        /// Parses as enum.
        /// </summary>
        public static T ToEnum<T>(this string @this)
        {
            try
            {
                return (T) Enum.Parse(typeof(T), @this);
            }
            catch
            {
                return default(T);
            }
        }
    }
}