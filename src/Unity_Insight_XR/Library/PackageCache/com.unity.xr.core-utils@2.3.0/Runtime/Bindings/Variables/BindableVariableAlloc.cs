using System;

namespace Unity.XR.CoreUtils.Bindings.Variables
{
    /// <summary>
    /// Generic class which contains a member variable of type <typeparamref name="T"/> and provides a binding API to data changes.
    /// If <typeparamref name="T"/> is <c>IEquatable</c>, use <see cref="BindableVariable{T}"/> instead.
    /// </summary>
    /// <typeparam name="T">The type of the variable value.</typeparam>
    /// <remarks>
    /// This class can be used for types which are not <c>IEquatable</c>.
    /// Since <typeparamref name="T"/> is not <c>IEquatable</c>, when setting the value,
    /// it calls <c>object.Equals</c> and will GC alloc.
    /// </remarks>
    /// <seealso cref="BindableVariable{T}"/>
    public class BindableVariableAlloc<T> : BindableVariableBase<T>
    {
        /// <summary>
        /// Constructor for bindable variable of type <typeparamref name="T"/>, which is a variable that notifies listeners when the internal value changes.
        /// </summary>
        /// <param name="initialValue">Value of type <typeparamref name="T"/> to initialize variable with. Defaults to type <see langword="default" />.</param>
        /// <param name="checkEquality">Setting <see langword="true"/> checks whether to compare new value to old before triggering callback. Defaults to <see langword="true"/>.</param>
        /// <param name="equalityMethod">Func used to provide custom equality checking behavior. Defaults to <c>Equals</c> check.</param>
        /// <param name="startInitialized">Setting <see langword="false"/> results in initial value setting will trigger registered callbacks, regardless of whether the value is the same as the initial one. Defaults to <see langword="false"/>.</param>
        public BindableVariableAlloc(T initialValue = default, bool checkEquality = true, Func<T, T, bool> equalityMethod = null, bool startInitialized = false)
            : base(initialValue, checkEquality, equalityMethod, startInitialized)
        {
        }
    }
}
