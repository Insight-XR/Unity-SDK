using System;

namespace Unity.XR.CoreUtils.Bindings.Variables
{
    /// <summary>
    /// Class which contains an <see langword="enum"/> member variable of type <typeparamref name="T"/> and provides a binding API to data changes.
    /// </summary>
    /// <remarks>
    /// Uses <c>GetHashCode</c> for comparison since <c>Equals</c> on an <c>enum</c> causes GC alloc.
    /// </remarks>
    /// <typeparam name="T">The type of the variable enum.</typeparam>
    public class BindableEnum<T> : BindableVariableBase<T> where T : struct, IConvertible
    {
        /// <summary>
        /// Constructor for bindable enum, which is a variable that notifies listeners when the internal enum value changes.
        /// </summary>
        /// <param name="initialValue">Enum value of type <typeparamref name="T"/> to initialize enum with. Defaults to type <see langword="default" />.</param>
        /// <param name="checkEquality">Setting <see langword="true"/> checks whether to compare new enum to old before triggering callback. Defaults to <see langword="true"/>.</param>
        /// <param name="equalityMethod">Func used to provide custom equality checking behavior. Defaults to <c>Equals</c> check.</param>
        /// <param name="startInitialized">Setting <see langword="false"/> results in initial enum setting will trigger registered callbacks, regardless of whether the value is the same as the initial one. Defaults to <see langword="false"/>.</param>
        public BindableEnum(T initialValue = default, bool checkEquality = true, Func<T, T, bool> equalityMethod = null, bool startInitialized = false)
            : base(initialValue, checkEquality, equalityMethod, startInitialized) { }

        /// <summary>
        /// Performs equal operation by comparing hash codes.
        /// </summary>
        /// <param name="other">Other enum to compare with.</param>
        /// <returns>Returns <see langword="true"/> if equal, returns <see langword="false"/> otherwise.</returns>
        public override bool ValueEquals(T other) => Value.GetHashCode() == other.GetHashCode();
    }
}
