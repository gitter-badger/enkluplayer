using System;

namespace CreateAR.EnkluPlayer
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
        /// <param name="defaultValue">Default value.</param>
        /// <returns></returns>
        public static T Parse<T>(string value, T defaultValue = default(T)) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

#if NETFX_CORE
                T type;
                if (Enum.TryParse(value, out type))
                {
                    return type;
                }

                return defaultValue;
#else
            try
            {
                return (T) Enum.Parse(
                    typeof(T),
                    value);
            }
            catch
            {
                return defaultValue;
            }
#endif
        }
    }
}
