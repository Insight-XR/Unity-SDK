using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// <inheritdoc cref="SmallRegistrationList{T}"/>
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="SmallRegistrationList{T}"/> Should be an interface.</typeparam>
    /// <remarks>
    /// <inheritdoc cref="SmallRegistrationList{T}"/>
    /// <para>
    /// A small registration list that can be exposed to users through the public interface <see cref="IXRFilterList{T}"/>.
    /// </para>
    /// </remarks>
    class ExposedRegistrationList<T> : SmallRegistrationList<T>, IXRFilterList<T> where T : class
    {
        public int count => flushedCount;

        public void Add(T item)
        {
            if (item == null || (item is Object unityObj && unityObj == null))
                throw new ArgumentNullException(nameof(item));

            Register(item);
        }

        public bool Remove(T item) => Unregister(item);

        public void MoveTo(T item, int newIndex) => MoveItemImmediately(item, newIndex);

        public void Clear() => UnregisterAll();

        public void GetAll(List<T> results) => GetRegisteredItems(results);


        public T GetAt(int index) => GetRegisteredItemAt(index);

        /// <summary>
        /// Register the given <paramref name="references"/> in this registration list.
        /// </summary>
        /// <param name="references">The list of items to add to the end of this list.</param>
        /// <param name="context">(Optional) the object context, ony used when an error needs to be thrown in the Console window.</param>
        /// <typeparam name="TObject">The references type.</typeparam>
        /// <remarks>
        /// If an element in the <paramref name="references"/> does not implement the interface <typeparamref name="T"/>,
        /// an error is thrown in the Console window.
        /// </remarks>
        /// <seealso cref="Add"/>
        public void RegisterReferences<TObject>(List<TObject> references, Object context = null) where TObject : Object
        {
            foreach (var reference in references)
            {
                if (reference != null && reference is T item)
                    Add(item);
                else if (context != null)
                    Debug.LogError($"Trying to add the invalid item {reference} into {typeof(IXRFilterList<T>).Name}, in {context}. {reference} does not implement {typeof(T).Name}.", context);
                else
                    Debug.LogError($"Trying to add the invalid item {reference} into {typeof(IXRFilterList<T>).Name}. {reference} does not implement {typeof(T).Name}.");
            }
        }
    }
}
