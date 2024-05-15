using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    /// <typeparam name="T"><inheritdoc /></typeparam>
    /// <remarks>
    /// <inheritdoc />
    /// <para>
    /// This is a variation of <see cref="RegistrationList{T}"/> that can be used when changes in registration
    /// is done infrequently. This class is also smaller in size since it does not have <see cref="HashSet{T}"/> fields
    /// which are more useful when the number of registered items is large and when registration changes happens
    /// more frequently.
    /// </para>
    /// </remarks>
    /// <seealso cref="RegistrationList{T}"/>
    class SmallRegistrationList<T> : BaseRegistrationList<T>
    {
        bool m_BufferChanges = true;

        /// <summary>
        /// Whether this list should buffer changes, the default value is <see langword="true"/>.
        /// Assign a <see langword="false"/> value to make all changes take effect immediately. This is useful when
        /// changes should be buffered only when the list is being processed.
        /// </summary>
        /// <remarks>
        /// When assign a value, and if needed, this property automatically calls <see cref="Flush"/> to guarantee the
        /// order of buffered changes.
        /// </remarks>
        public bool bufferChanges
        {
            get => m_BufferChanges;
            set
            {
                if (m_BufferChanges && value == false)
                {
                    m_BufferChanges = false;
                    Flush();
                }
                else
                {
                    m_BufferChanges = value;
                }
            }
        }

        /// <inheritdoc />
        public override bool IsRegistered(T item) => (bufferedAddCount > 0 && m_BufferedAdd.Contains(item)) ||
            (registeredSnapshot.Count > 0 && registeredSnapshot.Contains(item) && IsStillRegistered(item));

        /// <inheritdoc />
        public override bool IsStillRegistered(T item) => bufferedRemoveCount == 0 || !m_BufferedRemove.Contains(item);

        /// <inheritdoc />
        public override bool Register(T item)
        {
            if (!bufferChanges)
            {
                if (registeredSnapshot.Contains(item))
                    return false;

                registeredSnapshot.Add(item);
                return true;
            }

            if (bufferedAddCount > 0 && m_BufferedAdd.Contains(item))
                return false;

            var snapshotContainsItem = registeredSnapshot.Contains(item);
            if ((bufferedRemoveCount > 0 && RemoveFromBufferedRemove(item)) || !snapshotContainsItem)
            {
                if (!snapshotContainsItem)
                    AddToBufferedAdd(item);

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Unregister(T item)
        {
            if (!bufferChanges)
                return registeredSnapshot.Remove(item);

            if (bufferedRemoveCount > 0 && m_BufferedRemove.Contains(item))
                return false;

            if (bufferedAddCount > 0 && RemoveFromBufferedAdd(item))
                return true;

            if (registeredSnapshot.Contains(item))
            {
                AddToBufferedRemove(item);
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
                }

                ClearBufferedRemove();
            }

            if (bufferedAddCount > 0)
            {
                foreach (var item in m_BufferedAdd)
                {
                    if (!registeredSnapshot.Contains(item))
                    {
                        registeredSnapshot.Add(item);
                    }
                }

                ClearBufferedAdd();
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
                if (bufferedRemoveCount > 0 && m_BufferedRemove.Contains(item))
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
                if (bufferedRemoveCount > 0 && m_BufferedRemove.Contains(item))
                    continue;

                if (effectiveIndex == index)
                    return registeredSnapshot[index];

                ++effectiveIndex;
            }

            // Unreachable code
            throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the registration collection.");
        }
    }
}
