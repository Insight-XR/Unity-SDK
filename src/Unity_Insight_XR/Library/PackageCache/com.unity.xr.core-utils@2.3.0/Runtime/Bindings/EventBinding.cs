using System;

namespace Unity.XR.CoreUtils.Bindings
{
    /// <summary>
    /// Container struct holding a bind and unbind action.
    /// Useful for storing bind and unbind actions at the point of registration to avoid keeping track of the
    /// binding signature, and works with anonymous functions.
    /// </summary>
    public struct EventBinding : IEventBinding
    {
        /// <summary>
        /// Action to bind to callback.
        /// </summary>
        public Action BindAction { get; set; }

        /// <summary>
        /// Action to unbind from callback.
        /// </summary>
        public Action UnbindAction { get; set; }

        /// <inheritdoc/>
        public bool IsBound => m_IsBound;

        bool m_IsBound;

        /// <summary>
        /// Create an event binding container.
        /// </summary>
        /// <param name="bindAction">Action to initiate <see cref="Bind"/> (subscribe).</param>
        /// <param name="unBindAction">Action to initiate <see cref="Unbind"/> (unsubscribe).</param>
        public EventBinding(Action bindAction, Action unBindAction)
        {
            BindAction = bindAction;
            UnbindAction = unBindAction;
            m_IsBound = false;
        }

        /// <inheritdoc/>
        public void Bind()
        {
            if (!m_IsBound)
                BindAction?.Invoke();

            m_IsBound = true;
        }

        /// <inheritdoc/>
        public void Unbind()
        {
            if (m_IsBound)
                UnbindAction?.Invoke();

            m_IsBound = false;
        }

        /// <inheritdoc/>
        public void ClearBinding()
        {
            Unbind();
            BindAction = null;
            UnbindAction = null;
        }
    }
}
