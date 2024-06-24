using System;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine;

namespace Unity.XR.CoreUtils.Datums
{
    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a typed value.
    /// Can be referenced by multiple components in order to share the same set of data.
    /// </summary>
    /// <typeparam name="T">Value type held by this container.</typeparam>
    /// <seealso cref="DatumProperty{TValue,TDatum}"/>
    public abstract class Datum<T> : ScriptableObject
    {
        [Multiline]
        [SerializeField]
        string m_Comments;

        /// <summary>
        /// Comment that shows up in the Inspector window. Useful for explaining the purpose of the datum.
        /// </summary>
        public string Comments
        {
            get => m_Comments;
            set => m_Comments = value;
        }

        [SerializeField]
        bool m_ReadOnly = true;

        /// <summary>
        /// Controls whether the value in this datum is mutable or not.
        /// </summary>
        public bool ReadOnly
        {
            get => m_ReadOnly;
            set => m_ReadOnly = value;
        }

        [SerializeField]
        T m_Value;

        readonly BindableVariableAlloc<T> m_BindableVariableReference = new BindableVariableAlloc<T>();

        /// <summary>
        /// Read-only bindable variable reference that can be used for subscribing to value changes when not set to read-only.
        /// </summary>
        public IReadOnlyBindableVariable<T> BindableVariableReference => m_BindableVariableReference;

        /// <summary>
        /// Accessor for internal value.
        /// Setter only works if value is not read-only.
        /// </summary>
        public T Value
        {
            get => m_Value;
            set
            {
                if (m_ReadOnly)
                    Debug.LogWarning($"{this} ValueDatum is set to read-only, variable can't be changed!", this);
                else
                {
                    m_Value = value;
                    m_BindableVariableReference.Value = value;
                }
            }
        }

        /// <summary>
        /// This function is called when the object is loaded.
        /// Updates the value of the bindable variable reference.
        /// </summary>
        protected void OnEnable()
        {
            // Sync bindable variable ref
            m_BindableVariableReference.Value = Value;
        }
    }
}
