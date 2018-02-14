using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Because who doesn't need enum extensions.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Parses a string into an enum.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value">The string value.</param>
        /// <returns></returns>
        public static T Parse<T>(string value) where T : struct
        {
#if NETFX_CORE
                T type;
                if (Enum.TryParse(value, out type))
                {
                    return type;
                }

                return default(T);
#else
            try
            {
                return (T) Enum.Parse(
                    typeof(T),
                    value);
            }
            catch
            {
                return default(T);
            }
#endif
        }
    }
}
