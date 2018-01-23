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

        /// <summary>
        /// Created a new array with the specified element removed. If the array
        /// has more than one reference, all will be removed.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="this">The array to remove from.</param>
        /// <param name="element">The element to remvoe.</param>
        /// <returns></returns>
        public static T[] Remove<T>(this T[] @this, T element)
        {
            var arr = new T[@this.Length];
            var index = 0;
            for (int i = 0, len = @this.Length; i < len; i++)
            {
                if (!@this[i].Equals(element))
                {
                    arr[index++] = @this[i];
                }
            }

            if (index == @this.Length)
            {
                return arr;
            }

            var copy = new T[index];
            Array.Copy(arr, copy, index);

            return copy;
        }
    }
}