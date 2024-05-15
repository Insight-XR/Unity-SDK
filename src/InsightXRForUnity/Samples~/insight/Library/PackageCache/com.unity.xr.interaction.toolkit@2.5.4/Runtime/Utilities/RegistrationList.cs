using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// Use this class to maintain a registration of items (like Interactors or Interactables). This maintains
    /// a synchronized list that stays constant until buffered registration status changes are
    /// explicitly committed.
    /// </summary>
    /// <typeparam name="T">The type of object to register, such as <see cref="IXRInteractor"/> or <see cref="IXRInteractable"/>.</typeparam>
    /// <remarks>
    /// Items like Interactors and Interactables may be registered or unregistered (such as from an Interaction Manager)
    /// at any time, including when processing those items. This class can be used to manage those registration changes.
    /// For consistency with the functionality of Unity components which do not have
    /// Update called the same frame in which they are enabled, disabled, or destroyed,
    /// this class will maintain multiple lists to achieve that desired result with processing
    /// the items, these lists are pooled and reused between instances.
    /// </remarks>
    abstract class BaseRegistrationList<T>
    {
        /// <summary>
        /// Reusable list of buffered items (used to avoid unnecessary allocations when items to be added or removed
        /// need to be monitored).
        /// </summary>
        static readonly LinkedPool<List<T>> s_BufferedListPool = new LinkedPool<List<T>>
            (() => new List<T>(), actionOnRelease: list => list.Clear(), collectionCheck: false);

        /// <summary>
        /// A snapshot of registered items that should potentially be processed this update phase of the current frame.
        /// The count of items shall only change upon a call to <see cref="Flush"/>.
        /// </summary>
        /// <remarks>
        /// Items being in this collection does not imply that the item is currently registered.
        /// <br />
        /// Logically this should be a <see cref="IReadOnlyList{T}"/> but is kept as a <see cref="List{T}"/>
        /// to avoid allocations when iterating. Use <see cref="Register"/> and <see cref="Unregister"/>
        /// instead of directly changing this list.
        /// </remarks>
        public List<T> registeredSnapshot { get; } = new List<T>();

        /// <summary>
        /// Gets the number of registered items including the effective result of buffered registration changes.
        /// </summary>
        public int flushedCount => registeredSnapshot.Count - bufferedRemoveCount + bufferedAddCount;

        /// <summary>
        /// List with buffered items to be added when calling <see cref="Flush"/>.
        /// The count of items shall only change upon a call to <see cref="AddToBufferedAdd"/>,
        /// <see cref="RemoveFromBufferedAdd"/> or <see cref="ClearBufferedAdd"/>.
        /// This list can be <see langword="null"/>, use <see cref="bufferedAddCount"/> to check if there are elements
        /// before directly accessing it. A new list is pooled from <see cref="s_BufferedListPool"/> when needed.
        /// </summary>
        /// <remarks>
        /// Logically this should be a <see cref="IReadOnlyList{T}"/> but is kept as a <see cref="List{T}"/> to avoid
        /// allocations when iterating.
        /// </remarks>
        protected List<T> m_BufferedAdd;

        /// <summary>
        /// List with buffered items to be removed when calling <see cref="Flush"/>.
        /// The count of items shall only change upon a call to <see cref="AddToBufferedRemove"/>,
        /// <see cref="RemoveFromBufferedRemove"/> or <see cref="ClearBufferedRemove"/>.
        /// This list can be <see langword="null"/>, use <see cref="bufferedRemoveCount"/> to check if there are elements
        /// before directly accessing it. A new list is pooled from <see cref="s_BufferedListPool"/> when needed.
        /// </summary>
        /// <remarks>
        /// Logically this should be a <see cref="IReadOnlyList{T}"/> but is kept as a <see cref="List{T}"/> to avoid
        /// allocations when iterating.
        /// </remarks>
        protected List<T> m_BufferedRemove;

        /// <summary>
        /// The number of buffered items to be added when calling <see cref="Flush"/>.
        /// </summary>
        protected int bufferedAddCount => m_BufferedAdd?.Count ?? 0;

        /// <summary>
        /// The number of buffered items to be removed when calling <see cref="Flush"/>.
        /// </summary>
        protected int bufferedRemoveCount => m_BufferedRemove?.Count ?? 0;

        /// <summary>
        /// Adds the given item to the <see cref="m_BufferedAdd"/> list.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        /// <remarks>
        /// Gets a new list from the <see cref="s_BufferedListPool"/> if needed.
        /// </remarks>
        protected void AddToBufferedAdd(T item)
        {
            if (m_BufferedAdd == null)
                m_BufferedAdd = s_BufferedListPool.Get();

            m_BufferedAdd.Add(item);
        }

        /// <summary>
        /// Removes the given item from the <see cref="m_BufferedAdd"/> list.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns>Returns <see langword="true"/> if the item was successfully removed. Otherwise, returns <see langword="false"/>.</returns>
        protected bool RemoveFromBufferedAdd(T item) => m_BufferedAdd != null && m_BufferedAdd.Remove(item);

        /// <summary>
        /// Removes all items from the <see cref="m_BufferedAdd"/> and returns this list to the pool (<see cref="s_BufferedListPool"/>).
        /// </summary>
        protected void ClearBufferedAdd()
        {
            if (m_BufferedAdd == null)
                return;

            s_BufferedListPool.Release(m_BufferedAdd);
            m_BufferedAdd = null;
        }

        /// <summary>
        /// Adds the given item to the <see cref="m_BufferedRemove"/> list.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        /// <remarks>
        /// Gets a new list from the <see cref="s_BufferedListPool"/> if needed.
        /// </remarks>
        protected void AddToBufferedRemove(T item)
        {
            if (m_BufferedRemove == null)
                m_BufferedRemove = s_BufferedListPool.Get();

            m_BufferedRemove.Add(item);
        }

        /// <summary>
        /// Removes the given item from the <see cref="m_BufferedRemove"/> list.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns>Returns <see langword="true"/> if the item was successfully removed. Otherwise, returns <see langword="false"/>.</returns>
        protected bool RemoveFromBufferedRemove(T item) => m_BufferedRemove != null && m_BufferedRemove.Remove(item);

        /// <summary>
        /// Removes all items from the <see cref="m_BufferedRemove"/> and returns tis list to the pool (<see cref="s_BufferedListPool"/>).
        /// </summary>
        protected void ClearBufferedRemove()
        {
            if (m_BufferedRemove == null)
                return;

            s_BufferedListPool.Release(m_BufferedRemove);
            m_BufferedRemove = null;
        }

        /// <summary>
        /// Checks the registration status of <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to query.</param>
        /// <returns>Returns <see langword="true"/> if registered. Otherwise, returns <see langword="false"/>.</returns>
        /// <remarks>
        /// This includes pending changes that have not yet been pushed to <see cref="registeredSnapshot"/>.
        /// </remarks>
        /// <seealso cref="IsStillRegistered"/>
        public abstract bool IsRegistered(T item);

        /// <summary>
        /// Faster variant of <see cref="IsRegistered"/> that assumes that the <paramref name="item"/> is in the snapshot.
        /// It short circuits the check when there are no pending changes to unregister, which is usually the case.
        /// </summary>
        /// <param name="item">The item to query.</param>
        /// <returns>Returns <see langword="true"/> if registered</returns>
        /// <remarks>
        /// This includes pending changes that have not yet been pushed to <see cref="registeredSnapshot"/>.
        /// Use this method instead of <see cref="IsRegistered"/> when iterating over <see cref="registeredSnapshot"/>
        /// for improved performance.
        /// </remarks>
        /// <seealso cref="IsRegistered"/>
        public abstract bool IsStillRegistered(T item);

        /// <summary>
        /// Register <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to register.</param>
        /// <returns>Returns <see langword="true"/> if a change in registration status occurred. Otherwise, returns <see langword="false"/>.</returns>
        public abstract bool Register(T item);

        /// <summary>
        /// Unregister <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to unregister.</param>
        /// <returns>Returns <see langword="true"/> if a change in registration status occurred. Otherwise, returns <see langword="false"/>.</returns>
        public abstract bool Unregister(T item);

        /// <summary>
        /// Flush pending registration changes into <see cref="registeredSnapshot"/>.
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Return all registered items into List <paramref name="results"/> in the order they were registered.
        /// </summary>
        /// <param name="results">List to receive registered items.</param>
        /// <remarks>
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        public abstract void GetRegisteredItems(List<T> results);

        /// <summary>
        /// Returns the registered item at <paramref name="index"/> based on the order they were registered.
        /// </summary>
        /// <param name="index">Index of the item to return. Must be smaller than <see cref="flushedCount"/> and not negative.</param>
        /// <returns>Returns the item at the given index.</returns>
        public abstract T GetRegisteredItemAt(int index);

        /// <summary>
        /// Moves the given item in the registration list. Takes effect immediately without calling <see cref="Flush"/>.
        /// If the item is not in the registration list, this can be used to insert the item at the specified index.
        /// </summary>
        /// <param name="item">The item to move or register.</param>
        /// <param name="newIndex">New index of the item.</param>
        /// <returns>Returns <see langword="true"/> if the item was registered as a result of this method, otherwise returns <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Throws when there are pending registration changes that have not been flushed.</exception>
        public bool MoveItemImmediately(T item, int newIndex)
        {
            if (bufferedRemoveCount != 0 || bufferedAddCount != 0)
                throw new InvalidOperationException("Cannot move item when there are pending registration changes that have not been flushed.");

            var currentIndex = registeredSnapshot.IndexOf(item);
            if (currentIndex == newIndex)
                return false;

            if (currentIndex >= 0)
                registeredSnapshot.RemoveAt(currentIndex);

            registeredSnapshot.Insert(newIndex, item);
            OnItemMovedImmediately(item, newIndex);
            return currentIndex < 0;
        }

        /// <summary>
        /// Called after the given item has been inserted at or moved to the specified index.
        /// </summary>
        /// <param name="item">The item that was moved or registered.</param>
        /// <param name="newIndex">New index of the item.</param>
        protected virtual void OnItemMovedImmediately(T item, int newIndex)
        {
        }

        /// <summary>
        /// Unregister all currently registered items. Starts from the last registered item and proceeds backward
        /// until the first registered item is unregistered.
        /// </summary>
        public void UnregisterAll()
        {
            using (s_BufferedListPool.Get(out var registeredItems))
            {
                GetRegisteredItems(registeredItems);
                for (var i = registeredItems.Count - 1; i >= 0; --i)
                    Unregister(registeredItems[i]);
            }
        }

        protected static void EnsureCapacity(List<T> list, int capacity)
        {
            if (list.Capacity < capacity)
                list.Capacity = capacity;
        }
    }

    /// <inheritdoc />
    class RegistrationList<T> : BaseRegistrationList<T>
    {
        readonly HashSet<T> m_UnorderedBufferedAdd = new HashSet<T>();
        readonly HashSet<T> m_UnorderedBufferedRemove = new HashSet<T>();
        readonly HashSet<T> m_UnorderedRegisteredSnapshot = new HashSet<T>();
        readonly HashSet<T> m_UnorderedRegisteredItems = new HashSet<T>();

        /// <inheritdoc />
        public override bool IsRegistered(T item) => m_UnorderedRegisteredItems.Contains(item);

        /// <inheritdoc />
        public override bool IsStillRegistered(T item) => m_UnorderedBufferedRemove.Count == 0 || !m_UnorderedBufferedRemove.Contains(item);

        /// <inheritdoc />
        public override bool Register(T item)
        {
            if (m_UnorderedBufferedAdd.Count > 0 && m_UnorderedBufferedAdd.Contains(item))
                return false;

            var snapshotContainsItem = m_UnorderedRegisteredSnapshot.Contains(item);
            if ((m_UnorderedBufferedRemove.Count > 0 && m_UnorderedBufferedRemove.Remove(item)) || !snapshotContainsItem)
            {
                RemoveFromBufferedRemove(item);
                m_UnorderedRegisteredItems.Add(item);
                if (!snapshotContainsItem)
                {
                    AddToBufferedAdd(item);
                    m_UnorderedBufferedAdd.Add(item);
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Unregister(T item)
        {
            if (m_UnorderedBufferedRemove.Count > 0 && m_UnorderedBufferedRemove.Contains(item))
                return false;

            if (m_UnorderedBufferedAdd.Count > 0 && m_UnorderedBufferedAdd.Remove(item))
            {
                RemoveFromBufferedAdd(item);
                m_UnorderedRegisteredItems.Remove(item);
                return true;
            }

            if (m_UnorderedRegisteredSnapshot.Contains(item))
            {
                AddToBufferedRemove(item);
                m_UnorderedBufferedRemove.Add(item);
                m_UnorderedRegisteredItems.Remove(item);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override void Flush()
        {
            // This method is called multiple times each frame,
            // so additional explicit Count checks are done for
            // performance.
            if (bufferedRemoveCount > 0)
            {
                foreach (var item in m_BufferedRemove)
                {
                    registeredSnapshot.Remove(item);
                    m_UnorderedRegisteredSnapshot.Remove(item);
                }

                ClearBufferedRemove();
                m_UnorderedBufferedRemove.Clear();
            }

            if (bufferedAddCount > 0)
            {
                foreach (var item in m_BufferedAdd)
                {
                    if (!m_UnorderedRegisteredSnapshot.Contains(item))
                    {
                        registeredSnapshot.Add(item);
                        m_UnorderedRegisteredSnapshot.Add(item);
                    }
                }

                ClearBufferedAdd();
                m_UnorderedBufferedAdd.Clear();
            }
        }

        /// <inheritdoc />
        public override void GetRegisteredItems(List<T> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();
            EnsureCapacity(results, flushedCount);
            foreach (var item in registeredSnapshot)
            {
                if (m_UnorderedBufferedRemove.Count > 0 && m_UnorderedBufferedRemove.Contains(item))
                    continue;

                results.Add(item);
            }

            if (bufferedAddCount > 0)
                results.AddRange(m_BufferedAdd);
        }

        /// <inheritdoc />
        public override T GetRegisteredItemAt(int index)
        {
            if (index < 0 || index >= flushedCount)
                throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the registration collection.");

            if (bufferedRemoveCount == 0 && bufferedAddCount == 0)
                return registeredSnapshot[index];

            if (index >= registeredSnapshot.Count - bufferedRemoveCount)
                return m_BufferedAdd[index - (registeredSnapshot.Count - bufferedRemoveCount)];

            var effectiveIndex = 0;
            foreach (var item in registeredSnapshot)
            {
                if (m_UnorderedBufferedRemove.Contains(item))
                    continue;

                if (effectiveIndex == index)
                    return registeredSnapshot[index];

                ++effectiveIndex;
            }

            // Unreachable code
            throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the registration collection.");
        }

        /// <inheritdoc />
        protected override void OnItemMovedImmediately(T item, int newIndex)
        {
            base.OnItemMovedImmediately(item, newIndex);
            m_UnorderedRegisteredItems.Add(item);
            m_UnorderedRegisteredSnapshot.Add(item);
        }
    }
}
