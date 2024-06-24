using System.Collections.Generic;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for <see cref="HashSet{T}"/> objects.
    /// </summary>
    public static class HashSetExtensions
    {
        /// <summary>
        /// Remove any elements in this set that are in the set specified by <paramref name="other"/>. 
        /// </summary>
        /// <remarks>
        /// Equivalent to <see cref="HashSet{T}.ExceptWith(IEnumerable{T})"/>, but without any allocation.
        /// </remarks>
        /// <param name="self">The set from which to remove elements.</param>
        /// <param name="other">The set of elements to remove.</param>
        /// <typeparam name="T">The type contained in the set.</typeparam>
        public static void ExceptWithNonAlloc<T>(this HashSet<T> self, HashSet<T> other)
        {
            foreach (var entry in other)
                self.Remove(entry);
        }

        /// <summary>
        /// Gets the first element of a HashSet.
        /// </summary>
        /// <remarks>
        /// Equivalent to the <see cref="System.Linq"/> `.First()` method, but does not allocate.
        /// </remarks>
        /// <param name="set">Set to retrieve the element from</param>
        /// <typeparam name="T">Type contained in the set</typeparam>
        /// <returns>The first element in the set</returns>
        public static T First<T>(this HashSet<T> set)
        {
            var enumerator = set.GetEnumerator();
            var value = enumerator.MoveNext() ? enumerator.Current : default;
            enumerator.Dispose();
            return value;
        }
    }
}
