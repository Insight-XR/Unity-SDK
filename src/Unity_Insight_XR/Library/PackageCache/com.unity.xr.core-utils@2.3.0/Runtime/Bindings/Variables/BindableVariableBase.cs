using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.XR.CoreUtils.Bindings.Variables
{
    /// <summary>
    /// Abstract class which contains a member variable of type <typeparamref name="T"/> and provides a binding API to data changes.
    /// </summary>
    /// <typeparam name="T">The type of the variable value.</typeparam>
    /// <seealso cref="BindableVariable{T}"/>
    /// <seealso cref="BindableVariableAlloc{T}"/>
    /// <seealso cref="BindableEnum{T}"/>
    [Serializable]
    public abstract class BindableVariableBase<T> : IReadOnlyBindableVariable<T>
    {
        event Action<T> valueUpdated;

        T m_InternalValue;
        readonly bool m_CheckEquality;
        bool m_IsInitialized;
        readonly Func<T, T, bool> m_EqualityMethod;
        int m_BindingCount;

        /// <summary>
        /// The internal variable value. When setting the value, subscribers may be notified.
        /// The subscribers will not be notified if this variable is initialized, is configured to check for equality,
        /// and the new value is equivalent.
        /// </summary>
        /// <seealso cref="SetValueWithoutNotify"/>
        public T Value
        {
            get => m_InternalValue;
            set
            {
                if (SetValueWithoutNotify(value))
                    BroadcastValue();
            }
        }

        /// <summary>
        /// Get number of subscribed binding callbacks.
        /// Note that if you manually call <see cref="Unsubscribe"/> with the same callback several times this value may be inaccurate.
        /// For best results leverage the <see cref="IEventBinding"/> returned by the subscribe call and use that to unsubscribe as needed.
        /// </summary>
        public int BindingCount => m_BindingCount;

        /// <summary>
        /// Sets the internal variable value and, even if different, doesn't notify of the change to subscribed listeners.
        /// This is intended to be used by developers to allow for setting multiple bindable variables before broadcasting any of them individually.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns>Returns <see langword="true"/> if a broadcast would have normally occurred if the property setter was used instead.</returns>
        /// <seealso cref="Value"/>
        public bool SetValueWithoutNotify(T value)
        {
            if (m_IsInitialized && m_CheckEquality && (m_EqualityMethod?.Invoke(m_InternalValue, value) ?? ValueEquals(value)))
                return false;

            m_IsInitialized = true;
            m_InternalValue = value;
            return true;
        }

        /// <inheritdoc />
        public IEventBinding Subscribe(Action<T> callback)
        {
            var newBinding = new EventBinding();
            if (callback != null)
            {
                var callbackReference = callback;
                newBinding.BindAction = () =>
                {
                    valueUpdated += callbackReference;
                    IncrementReferenceCount();
                };
                newBinding.UnbindAction = () =>
                {
                    valueUpdated -= callbackReference;
                    DecrementReferenceCount();
                };
                newBinding.Bind();
            }

            return newBinding;
        }

        /// <inheritdoc />
        public IEventBinding SubscribeAndUpdate(Action<T> callback)
        {
            callback?.Invoke(m_InternalValue);

            return Subscribe(callback);
        }

        /// <inheritdoc />
        public void Unsubscribe(Action<T> callback)
        {
            if (callback != null)
            {
                valueUpdated -= callback;
                DecrementReferenceCount();
            }
        }

        void IncrementReferenceCount()
        {
            m_BindingCount++;
        }

        void DecrementReferenceCount()
        {
            m_BindingCount = Mathf.Max(0, m_BindingCount - 1);
        }

        /// <summary>
        /// Constructor for bindable variable, which is a variable that notifies listeners when the internal value changes.
        /// </summary>
        /// <param name="initialValue">Value to initialize variable with. Defaults to type default.</param>
        /// <param name="checkEquality">Setting true checks whether to compare new value to old before triggering callback. Defaults to <see langword="true"/>.</param>
        /// <param name="equalityMethod">Func used to provide custom equality checking behavior. Defaults to <c>Equals</c> check.</param>
        /// <param name="startInitialized">Setting false results in initial value setting will trigger registered callbacks, regardless of whether the value is the same as the initial one. Defaults to <see langword="false"/>.</param>
        protected BindableVariableBase(T initialValue = default, bool checkEquality = true, Func<T, T, bool> equalityMethod = null, bool startInitialized = false)
        {
            m_IsInitialized = startInitialized;
            m_InternalValue = initialValue;
            m_CheckEquality = checkEquality;
            m_EqualityMethod = equalityMethod;
            m_BindingCount = 0;
        }

        /// <summary>
        /// Triggers a callback for all subscribed listeners with the current internal variable value.
        /// </summary>
        public void BroadcastValue()
        {
            valueUpdated?.Invoke(m_InternalValue);
        }

        /// <inheritdoc />
        public Task<T> Task(Func<T, bool> awaitPredicate, CancellationToken token = default)
        {
            if (awaitPredicate != null && awaitPredicate.Invoke(m_InternalValue))
                return System.Threading.Tasks.Task.FromResult(m_InternalValue);

            return new BindableVariableTaskPredicate<T>(this, awaitPredicate, token).Task;
        }

        /// <inheritdoc />
        public Task<T> Task(T awaitState, CancellationToken token = default)
        {
            if (ValueEquals(awaitState))
                return System.Threading.Tasks.Task.FromResult(m_InternalValue);

            return new BindableVariableTaskState<T>(this, awaitState, token).task;
        }

        /// <inheritdoc />
        public virtual bool ValueEquals(T other) => m_InternalValue.Equals(other);
    }
}
