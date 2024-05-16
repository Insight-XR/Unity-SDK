using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// A list of items.
    /// </summary>
    /// <typeparam name="T">The type of the items in this list.</typeparam>
    public interface IXRFilterList<T>
    {
        /// <summary>
        /// The number of items in this list.
        /// </summary>
        int count { get; }

        /// <summary>
        /// Adds the given item to the end of this list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void Add(T item);

        /// <summary>
        /// Removes the given item from this list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="item"/> was removed from the list.
        /// Otherwise, returns <see langword="false"/> if the <paramref name="item"/> was not found.
        /// </returns>
        bool Remove(T item);

        /// <summary>
        /// Moves the given item in this list.
        /// If the given item is not in this list, this can be used to insert the item at the specified index.
        /// </summary>
        /// <param name="item">The item to move or add.</param>
        /// <param name="newIndex">New index of the item.</param>
        void MoveTo(T item, int newIndex);

        /// <summary>
        /// Removes all items from this list.
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns all items into List <paramref name="results"/>.
        /// </summary>
        /// <param name="results">List to receive all items.</param>
        /// <remarks>
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        void GetAll(List<T> results);

        /// <summary>
        /// Returns the item at <paramref name="index"/> in this list.
        /// </summary>
        /// <param name="index">Index of the item to return. Must be smaller than <see cref="count"/> and not negative.</param>
        /// <returns>Returns the item at the given index.</returns>
        T GetAt(int index);
    }
}
