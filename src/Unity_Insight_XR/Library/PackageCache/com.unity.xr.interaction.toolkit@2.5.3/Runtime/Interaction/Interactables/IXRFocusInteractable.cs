using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that represents an Interactable component which
    /// an Interactor component can persistently select.
    /// </summary>
    public interface IXRFocusInteractable : IXRInteractable
    {
        /// <summary>
        /// The event that is called only when the first Interaction Group gains focus on
        /// this Interactable as the sole focusing Interactor. Subsequent Interactors that
        /// gain focus will not cause this event to be invoked as long as any others are still focusing.
        /// </summary>
        /// <remarks>
        /// The <see cref="FocusEnterEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="lastFocusExited"/>
        /// <seealso cref="focusEntered"/>
        FocusEnterEvent firstFocusEntered { get; }

        /// <summary>
        /// The event that is called only when the last remaining focused Interaction group
        /// loses focus on this Interactable.
        /// </summary>
        /// <remarks>
        /// The <see cref="FocusExitEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="firstFocusEntered"/>
        /// <seealso cref="focusExited"/>
        FocusExitEvent lastFocusExited { get; }

        /// <summary>
        /// The event that is called when an Interaction group gains focus on this Interactable.
        /// </summary>
        /// <remarks>
        /// The <see cref="FocusEnterEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="focusExited"/>
        FocusEnterEvent focusEntered { get; }

        /// <summary>
        /// The event that is called when an Interaction group loses focus on this Interactable.
        /// </summary>
        /// <remarks>
        /// The <see cref="FocusExitEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="focusEntered"/>
        FocusExitEvent focusExited { get; }

        /// <summary>
        /// (Read Only) The list of Interaction groups currently focusing on this Interactable (may by empty).
        /// </summary>
        /// <remarks>
        /// You should treat this as a read only view of the list and should not modify it.
        /// It is exposed as a <see cref="List{T}"/> rather than an <see cref="IReadOnlyList{T}"/> to avoid GC Allocations
        /// when enumerating the list.
        /// </remarks>
        /// <seealso cref="isFocused"/>
        List<IXRInteractionGroup> interactionGroupsFocusing { get; }

        /// <summary>
        /// (Read Only) The first interaction group that is focused on this interactable since not being focused.
        /// The group may not currently be focusing this interactable, which would be the case
        /// when it released while multiple groups were focusing this interactable.
        /// </summary>
        IXRInteractionGroup firstInteractionGroupFocusing { get; }

        /// <summary>
        /// (Read Only) Indicates whether this interactable is currently being focused by any interaction group.
        /// </summary>
        /// <remarks>
        /// In other words, returns whether <see cref="interactionGroupsFocusing"/> contains any interaction groups.
        /// <example>
        /// <code>interactionGroupsFocusing.Count > 0</code>
        /// </example>
        /// </remarks>
        /// <seealso cref="interactionGroupsFocusing"/>
        bool isFocused { get; }

        /// <summary>
        /// Indicates the focus policy of an Interactable.
        /// </summary>
        /// <seealso cref="InteractableFocusMode"/>
        InteractableFocusMode focusMode { get; }

        /// <summary>
        /// Indicates whether this Interactable can be focused.
        /// </summary>
        bool canFocus { get; }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interaction group first gains focus of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is initiating the focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnFocusEntered(FocusEnterEventArgs)"/>
        void OnFocusEntering(FocusEnterEventArgs args);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interaction group first gains focus of an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is initiating the focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnFocusExited(FocusExitEventArgs)"/>
        void OnFocusEntered(FocusEnterEventArgs args);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interaction group loses focus of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is losing focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnFocusExited(FocusExitEventArgs)"/>
        void OnFocusExiting(FocusExitEventArgs args);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interaction group loses focus of an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is losing focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnFocusEntered(FocusEnterEventArgs)"/>
        void OnFocusExited(FocusExitEventArgs args);
    }

    /// <summary>
    /// Extension methods for <see cref="IXRFocusInteractable"/>.
    /// </summary>
    /// <seealso cref="IXRFocusInteractable"/>
    public static class XRFocusInteractableExtensions
    {
        /// <summary>
        /// Gets the oldest interaction group currently focusing on this interactable.
        /// This is a convenience method for when the interactable does not support being focused by multiple interaction groups at a time.
        /// </summary>
        /// <param name="interactable">The interactable to operate on.</param>
        /// <returns>Returns the oldest interaction group currently focusing this interactable.</returns>
        /// <remarks>
        /// Equivalent to <code>interactionGroupsFocusing.Count > 0 ? interactionGroupsFocusing[0] : null</code>
        /// </remarks>
        public static IXRInteractionGroup GetOldestInteractorFocusing(this IXRFocusInteractable interactable) =>
            interactable?.interactionGroupsFocusing.Count > 0 ? interactable.interactionGroupsFocusing[0] : null;
    }

    /// <summary>
    /// Options for the focus policy of an Interactable.
    /// </summary>
    /// <seealso cref="IXRFocusInteractable.focusMode"/>
    public enum InteractableFocusMode
    {
        /// <summary>
        /// Focus not supported for this interactable.
        /// </summary>
        None,

        /// <summary>
        /// Allows the Interactable to only be focused by a single Interaction group at a time
        /// and allows other Interaction groups to take focus by automatically losing focus.
        /// </summary>
        Single,

        /// <summary>
        /// Allows for multiple Interaction groups at a time to focus the Interactable.
        /// </summary>
        /// <remarks>
        /// This option can be disabled in the Inspector window by adding the <see cref="CanFocusMultipleAttribute"/>
        /// with a value of <see langword="false"/> to a derived class of <see cref="XRBaseInteractable"/>.
        /// </remarks>
        Multiple,
    }
}