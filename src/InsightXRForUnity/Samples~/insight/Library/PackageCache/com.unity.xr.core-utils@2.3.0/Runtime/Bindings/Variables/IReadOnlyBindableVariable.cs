using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.XR.CoreUtils.Bindings.Variables
{
    /// <summary>
    /// Bindable variable interface useful for removing write capability from external access,
    /// while preserving the ability to subscribe to changes.
    /// </summary>
    /// <typeparam name="T">The type of the variable value.</typeparam>
    public interface IReadOnlyBindableVariable<T>
    {
        /// <summary>
        /// Register callback to the event that is invoked when the value is updated.
        /// </summary>
        /// <param name="callback">Callback to register.</param>
        /// <returns><see cref="IEventBinding"/> which allows for safe and easy bind, unbind, and clear functions.</returns>
        IEventBinding Subscribe(Action<T> callback);

        /// <summary>
        /// Triggers the callback inline, followed by the <see cref="Subscribe"/> function.
        /// </summary>
        /// <param name="callback">Callback to register.</param>
        /// <returns><see cref="IEventBinding"/> which allows for safe and easy bind, unbind, and clear functions.</returns>
        IEventBinding SubscribeAndUpdate(Action<T> callback);

        /// <summary>
        /// Manually unsubscribe callback from Value update event, but no protections from multiple unsubscribe calls.
        /// If unsubscribing multiple times, reference count may not be accurate.
        /// </summary>
        /// <param name="callback">Callback to unregister.</param>
        void Unsubscribe(Action<T> callback);

        /// <summary>
        /// Get internal variable value.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Get number of subscribed binding callbacks.
        /// Note that if you manually call <see cref="Unsubscribe"/> with the same callback several times this value may be inaccurate.
        /// For best results leverage the <see cref="IEventBinding"/> returned by the subscribe call and use that to unsubscribe as needed.
        /// </summary>
        int BindingCount { get; }

        /// <summary>
        /// Evaluates equality with the internal value held by the bindable variable.
        /// </summary>
        /// <param name="other">Other value to compare equality against.</param>
        /// <returns>True if both values are equal.</returns>
        bool ValueEquals(T other);

        /// <summary>
        /// Wait until predicate is met, or until token is called.
        /// A <see langword="null"/> predicate can be passed to have it await any change.
        /// </summary>
        /// <param name="awaitPredicate">Callback to be executed on completion of task.</param>
        /// <param name="token">Token used to trigger a cancellation of the task.</param>
        /// <returns>Task to schedule.</returns>
        Task<T> Task(Func<T, bool> awaitPredicate, CancellationToken token = default);

        /// <summary>
        /// Wait until BindableVariable is set to <paramref name="awaitState"/>.
        /// </summary>
        /// <param name="awaitState">Variable state to wait for.</param>
        /// <param name="token">Token used to trigger a cancellation of the task.</param>
        /// <returns>Task to schedule.</returns>
        Task<T> Task(T awaitState, CancellationToken token = default);
    }
}
