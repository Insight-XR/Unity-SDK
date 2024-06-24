using System.Collections.Generic;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for <see cref="Dictionary{TKey, TValue}"/> objects.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the first element in the dictionary.
        /// </summary>
        /// <remarks>
        /// Equivalent to the <see cref="System.Linq"/> `.First()` method, but does not allocate.
        /// </remarks>
        /// <param name="dictionary">Dictionary to retrieve the element from.</param>
        /// <typeparam name="TKey">Dictionary's Key type.</typeparam>
        /// <typeparam name="TValue">Dictionary's Value type.</typeparam>
        /// <returns>The first element in the dictionary.</returns>
        public static KeyValuePair<TKey, TValue> First<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            var kvp = default(KeyValuePair<TKey, TValue>);
            var enumerator = dictionary.GetEnumerator();
            if (enumerator.MoveNext())
            {
                kvp = enumerator.Current;
            }

            enumerator.Dispose();
            return kvp;
        }
    }
}
