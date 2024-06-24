using System.Collections.Generic;

namespace Unity.XR.CoreUtils.Bindings
{
    /// <summary>
    /// Container class for IEvent bindings. Helps unbind or clear bindings in bulk.
    /// </summary>
    public class BindingsGroup
    {
        readonly List<IEventBinding> m_Bindings = new List<IEventBinding>();

        /// <summary>
        /// Register binding to group
        /// </summary>
        /// <param name="binding">Binding to register</param>
        public void AddBinding(IEventBinding binding)
        {
            m_Bindings.Add(binding);
        }

        /// <summary>
        /// Clear a specific binding and remove it from the binding group.
        /// </summary>
        /// <param name="binding">Binding to clear</param>
        public void ClearBinding(IEventBinding binding)
        {
            m_Bindings.Remove(binding);
            binding?.ClearBinding();
        }

        /// <summary>
        /// Triggers binding action on all registered bindings.
        /// </summary>
        public void Bind()
        {
            for (int i = 0; i < m_Bindings.Count; i++)
            {
                m_Bindings[i]?.Bind();
            }
        }

        /// <summary>
        /// Triggers unbind action all registered bindings without clearing them.
        /// </summary>
        public void Unbind()
        {
            for (int i = 0; i < m_Bindings.Count; i++)
            {
                m_Bindings[i]?.Unbind();
            }
        }

        /// <summary>
        /// Triggers unbind action on all bindings and destroys all stored binding actions, as well as clears the
        /// group of all registered bindings.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < m_Bindings.Count; i++)
            {
                m_Bindings[i]?.ClearBinding();
            }

            m_Bindings.Clear();
        }
    }
}
