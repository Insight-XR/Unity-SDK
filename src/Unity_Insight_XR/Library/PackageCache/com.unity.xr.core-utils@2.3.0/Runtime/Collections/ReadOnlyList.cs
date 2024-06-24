using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.XR.CoreUtils.Collections
{
    /// <summary>
    /// Wraps a <see cref="List{T}"/> to provide a read-only view of its memory without copying any elements.
    /// It is preferable to use this collection in API designs instead of `IReadOnlyCollection` because
    /// <see cref="GetEnumerator"/> returns a value-type enumerator and does not perform any heap allocations.
    /// </summary>
    /// <remarks>
    /// This collection is not thread-safe.
    /// </remarks>
    /// <typeparam name="T">The element type.</typeparam>
    public class ReadOnlyList<T> : IReadOnlyList<T>
    {
        readonly List<T> m_List;

        /// <summary>
        /// The number of elements in the list.
        /// </summary>
        /// <value>The number of elements.</value>
        public int Count => m_List.Count;

        /// <summary>
        /// Returns the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index.</param>
        public T this[int index] => m_List[index];

        /// <summary>
        /// Constructs a new instance of this class that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> is <see langword="null"/>.</exception>
        public ReadOnlyList(List<T> list)
        {
            m_List = list ?? throw new ArgumentNullException(nameof(list));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public List<T>.Enumerator GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This implementation performs a boxing operation and should be avoided.
        /// > Use the public <see cref="GetEnumerator"/> overload instead.
        /// </remarks>
        /// <returns>The boxed enumerator.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the list.
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > This implementation performs a boxing operation and should be avoided.
        /// > Use the public <see cref="GetEnumerator"/> overload instead.
        /// </remarks>
        /// <returns>The boxed enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
