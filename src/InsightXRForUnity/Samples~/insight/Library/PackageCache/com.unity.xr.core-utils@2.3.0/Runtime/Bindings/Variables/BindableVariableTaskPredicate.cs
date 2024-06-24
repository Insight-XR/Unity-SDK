using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.XR.CoreUtils.Bindings.Variables
{
    /// <summary>
    /// Predicate container holding task with a callback on completion.
    /// </summary>
    /// <typeparam name="T">Type param</typeparam>
    struct BindableVariableTaskPredicate<T>
    {
        readonly TaskCompletionSource<T> m_Tcs;
        readonly Func<T, bool> m_AwaitPredicate;
        readonly IReadOnlyBindableVariable<T> m_BindableVariable;

        /// <summary>
        /// Internal task reference.
        /// </summary>
        public Task<T> Task => m_Tcs.Task;

        /// <summary>
        /// BindableVariableTaskPredicate constructor
        /// </summary>
        /// <param name="bindableVariable">Bindable variable to propagate task callbacks.</param>
        /// <param name="awaitPredicate">Callback to be executed on completion of task.</param>
        /// <param name="cancellationToken">Token used to trigger a cancellation of the task.</param>
        public BindableVariableTaskPredicate(IReadOnlyBindableVariable<T> bindableVariable, Func<T, bool> awaitPredicate, CancellationToken cancellationToken = default)
        {
            m_Tcs = new TaskCompletionSource<T>();
            m_AwaitPredicate = awaitPredicate;
            m_BindableVariable = bindableVariable;

            // If condition already met, just set immediately
            if (m_AwaitPredicate != null && m_AwaitPredicate(m_BindableVariable.Value))
            {
                m_Tcs.SetResult(m_BindableVariable.Value);
            }
            else
            {
                cancellationToken.Register(Cancelled);
                m_BindableVariable.Subscribe(Await);
            }
        }

        void Cancelled()
        {
            m_BindableVariable.Unsubscribe(Await);
            m_Tcs.SetResult(m_BindableVariable.Value);
        }

        void Await(T state)
        {
            if (m_AwaitPredicate != null)
            {
                if (m_AwaitPredicate(state))
                {
                    m_BindableVariable.Unsubscribe(Await);
                    m_Tcs.SetResult(state);
                }
            }
            else
            {
                m_BindableVariable.Unsubscribe(Await);
                m_Tcs.SetResult(state);
            }
        }
    }

    /// <summary>
    /// Structure for holding bindable variable task state.
    /// </summary>
    /// <typeparam name="T">Type param</typeparam>
    struct BindableVariableTaskState<T>
    {
        readonly TaskCompletionSource<T> m_Tcs;
        readonly T m_AwaitState;
        readonly IReadOnlyBindableVariable<T> m_BindableVariable;

        /// <summary>
        /// The task which we are waiting for completion.
        /// </summary>
        public Task<T> task => m_Tcs.Task;

        /// <summary>
        /// Constructor for BindableVariableTaskState. This will create a new <see cref="TaskCompletionSource{T}"/>
        /// and await the task completion.
        /// </summary>
        /// <param name="bindableVariable">The BindableVariable to monitor the value of for the proper state.</param>
        /// <param name="awaitState">The state to wait for to trigger the task complete.</param>
        /// <param name="cancellationToken">Cancellation token used to stop the current task before completion.</param>
        public BindableVariableTaskState(IReadOnlyBindableVariable<T> bindableVariable, T awaitState, CancellationToken cancellationToken = default)
        {
            m_Tcs = new TaskCompletionSource<T>();
            m_AwaitState = awaitState;
            m_BindableVariable = bindableVariable;

            // If condition already met, just set immediately
            if (m_BindableVariable.ValueEquals(awaitState))
            {
                m_Tcs.SetResult(m_BindableVariable.Value);
            }
            else
            {
                cancellationToken.Register(Cancelled);
                m_BindableVariable.Subscribe(Await);
            }
        }

        void Cancelled()
        {
            m_BindableVariable.Unsubscribe(Await);
            m_Tcs.SetResult(m_BindableVariable.Value);
        }

        void Await(T state)
        {
            if (m_BindableVariable.ValueEquals(m_AwaitState))
            {
                m_BindableVariable.Unsubscribe(Await);
                m_Tcs.SetResult(state);
            }
        }
    }
}
