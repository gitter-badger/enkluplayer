using System;
using Random = UnityEngine.Random;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Extensions for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Characters.
        /// </summary>
        private static readonly char[] _Characters = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

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

        /// <summary>
        /// Generates a random identifier of a specific length. Only includes
        /// capital letters.
        /// </summary>
        /// <param name="len">The length of the string.</param>
        /// <returns></returns>
        public static string RandomIdentifier(int len)
        {
            var str = new char[len];
            for (var i = 0; i < len; i++)
            {
                str[i] = _Characters[(int) Math.Floor(26 * Random.value)];
            }

            return new string(str);
        }
    }
}