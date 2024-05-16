using Unity.XR.CoreUtils.Bindings.Variables;

namespace Unity.XR.CoreUtils.Bindings
{
    /// <summary>
    /// Interface for event binding used by <see cref="BindableVariable{T}"/>.
    /// </summary>
    public interface IEventBinding
    {
        /// <summary>
        /// True if Bind function was called and binding is currently active.
        /// </summary>
        bool IsBound { get; }

        /// <summary>
        /// Trigger binding action.
        /// </summary>
        void Bind();

        /// <summary>
        /// Trigger unbinding action.
        /// </summary>
        void Unbind();

        /// <summary>
        /// Trigger unbinding action and then destroy all binding action references.
        /// </summary>
        void ClearBinding();
    }
}
