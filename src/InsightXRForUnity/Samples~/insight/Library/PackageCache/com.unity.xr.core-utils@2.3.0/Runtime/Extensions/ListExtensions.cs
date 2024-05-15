using System.Collections.Generic;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for <see cref="List{T}"/> objects.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Fills the list with default objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="list">The list to populate.</param>
        /// <param name="count">The number of items to add to the list.</param>
        /// <typeparam name="T">The type of objects in this list.</typeparam>
        /// <returns>The list that was filled.</returns>
        public static List<T> Fill<T>(this List<T> list, int count)
            where T: new()
        {
            for (var i = 0; i < count; i++)
            {
                list.Add(new T());
            }

            return list;
        }

        /// <summary>
        /// Ensures that the capacity of this list is at least as large the given value.
        /// </summary>
        /// <remarks>Increases the capacity of the list, if necessary, but doe not decrease the
        /// capacity if it already exceeds the specified value.</remarks>
        /// <typeparam name="T">The list element type.</typeparam>
        /// <param name="list">The list whose capacity will be ensured.</param>
        /// <param name="capacity">The minimum number of elements the list storage must contain.</param>
        public static void EnsureCapacity<T>(this List<T> list, int capacity)
        {
            if (list.Capacity < capacity)
                list.Capacity = capacity;
        }

        /// <summary>
        /// Swaps the elements at <paramref name="first"/> and <paramref name="second"/> with minimal copying.
        /// Works for any type of <see cref="List{T}"/>.
        /// </summary>
        /// <param name="list">The list to perform the swap on.</param>
        /// <param name="first">The index of the first item to swap.</param>
        /// <param name="second">The index of the second item to swap.</param>
        /// <typeparam name="T">The type of list items to swapped.</typeparam>
        public static void SwapAtIndices<T>(this List<T> list, int first, int second)
        {
            (list[first], list[second]) = (list[second], list[first]);
        }
    }
}
