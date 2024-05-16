using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils.Collections
{
    /// <summary>
    /// A dictionary class that can be serialized by Unity.
    /// Inspired by the implementation in http://answers.unity3d.com/answers/809221/view.html
    /// </summary>
    /// <typeparam name="TKey">The dictionary key.</typeparam>
    /// <typeparam name="TValue">The dictionary value.</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Class that stores the serialized items in this dictionary.
        /// </summary>
        [Serializable]
        public struct Item
        {
            /// <summary>
            /// The dictionary item key.
            /// </summary>
            public TKey Key;

            /// <summary>
            /// The dictionary item value.
            /// </summary>
            public TValue Value;
        }

        [SerializeField]
        List<Item> m_Items = new List<Item>();

        /// <summary>
        /// The serialized items in this dictionary.
        /// </summary>
        public List<Item> SerializedItems => m_Items;

        /// <summary>
        /// Initializes a new instance of the dictionary.
        /// </summary>
        public SerializableDictionary() { }

        /// <summary>
        /// Initializes a new instance of the dictionary that contains elements copied from the given
        /// <paramref name="input"/> dictionary.
        /// </summary>
        /// <param name="input">The dictionary from which to copy the elements.</param>
        public SerializableDictionary(IDictionary<TKey, TValue> input) : base(input) { }

        /// <summary>
        /// See <see cref="ISerializationCallbackReceiver"/>
        /// Save this dictionary to the <see cref="SerializedItems"/> list.
        /// </summary>
        public virtual void OnBeforeSerialize()
        {
            m_Items.Clear();
            foreach (var pair in this)
                m_Items.Add(new Item {Key = pair.Key, Value = pair.Value});
        }

        /// <summary>
        /// See <see cref="ISerializationCallbackReceiver"/>
        /// Load this dictionary from the <see cref="SerializedItems"/> list.
        /// </summary>
        public virtual void OnAfterDeserialize()
        {
            Clear();
            foreach (var item in m_Items)
            {
                if (ContainsKey(item.Key))
                {
                    Debug.LogWarning($"The key \"{item.Key}\" is duplicated in the {GetType().Name}.{nameof(SerializedItems)} and will be ignored.");
                    continue;
                }

                Add(item.Key, item.Value);
            }
        }
    }
}
