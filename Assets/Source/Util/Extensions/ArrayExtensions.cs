using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Extensions for arrays.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Creates a new array with the added element.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="this">The array to add to.</param>
        /// <param name="element">The element to add.</param>
        /// <returns></returns>
        public static T[] Add<T>(this T[] @this, T element)
        {
            var len = @this.Length;
            var copy = new T[len + 1];

            Array.Copy(@this, copy, len);

            copy[len] = element;

            return copy;
        }
    }
}