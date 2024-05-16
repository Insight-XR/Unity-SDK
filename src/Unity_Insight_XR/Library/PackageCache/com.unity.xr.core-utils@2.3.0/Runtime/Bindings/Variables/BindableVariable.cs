using System;

namespace Unity.XR.CoreUtils.Bindings.Variables
{
    /// <summary>
    /// Generic class which contains a member variable of type <typeparamref name="T"/> and provides a binding API to data changes.
    /// </summary>
    /// <typeparam name="T">The type of the variable value.</typeparam>
    /// <remarks>
    /// <typeparamref name="T"/> is <c>IEquatable</c> to avoid GC alloc that would occur with <c>object.Equals</c> in the base class.
    /// </remarks>
    public class BindableVariable<T> : BindableVariableBase<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Constructor for bindable variable of type <typeparamref name="T"/>, which is a variable that notifies listeners when the internal value changes.
        /// </summary>
        /// <param name="initialValue">Value of type <typeparamref name="T"/> to initialize variable with. Defaults to type <see langword="default" />.</param>
        /// <param name="checkEquality">Setting <see langword="true"/> checks whether to compare new value to old before triggering callback. Defaults to <see langword="true"/>.</param>
        /// <param name="equalityMethod">Func used to provide custom equality checking behavior. Defaults to <c>Equals</c> check.</param>
        /// <param name="startInitialized">Setting <see langword="false"/> results in initial value setting will trigger registered callbacks, regardless of whether the value is the same as the initial one. Defaults to <see langword="false"/>.</param>
        public BindableVariable(T initialValue = default, bool checkEquality = true, Func<T, T, bool> equalityMethod = null, bool startInitialized = false)
            : base(initialValue, checkEquality, equalityMethod, startInitialized)
        {
        }

        /// <inheritdoc />
        // Uses IEquatable<T>.Equals rather than object.Equals done in the base class to avoid GC alloc
        public override bool ValueEquals(T other) => Value.Equals(other);
    }
}
