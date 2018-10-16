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

        /// <summary>
        /// Calculates a hash of a string.
        /// </summary>
        /// <param name="this">The hash.</param>
        /// <returns></returns>
        public static ulong Crc64(this string @this)
        {
            var hashedValue = 3074457345618258791ul;
            for (int i = 0, len = @this.Length; i <len; i++)
            {
                hashedValue += @this[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    }
}