namespace UnityEngine.XR.Interaction.Toolkit.Transformers
{
    /// <summary>
    /// Event data associated with the event when an <see cref="XRGrabInteractable"/> is dropped by all interactors.
    /// </summary>
    /// <seealso cref="IXRDropTransformer.OnDrop"/>
    public sealed class DropEventArgs
    {
        /// <summary>
        /// The event data associated with the select exit event.
        /// </summary>
        public SelectExitEventArgs selectExitEventArgs { get; set; }
    }

    /// <summary>
    /// An interface that allows the target position, rotation, and scale of an <see cref="XRGrabInteractable"/> to be
    /// calculated. This interface adds the ability for the grab transformer to be notified when the interactable is dropped
    /// and to process once more.
    /// </summary>
    /// <seealso cref="IXRGrabTransformer"/>
    public interface IXRDropTransformer : IXRGrabTransformer
    {
        /// <summary>
        /// Whether this grab transformer opts-in to allowing <see cref="IXRGrabTransformer.Process"/> to be called
        /// by Unity once more after the interactable is deselected by all interactors.
        /// </summary>
        /// <remarks>
        /// When the grab transformer implements this interface and this property and <see cref="IXRGrabTransformer.canProcess"/>
        /// both returns <see langword="true"/>, the <see cref="IXRGrabTransformer.Process"/> method will be called once more after <see cref="OnDrop"/>.
        /// </remarks>
        /// <seealso cref="IXRGrabTransformer.canProcess"/>
        /// <seealso cref="IXRGrabTransformer.Process"/>
        bool canProcessOnDrop { get; }

        /// <summary>
        /// Called by Unity when the given Interactable is dropped (in other words, when exiting the Select state).
        /// This method won't be called until the Interactable is released by every Interactor.
        /// Use this to do any code deinitialization based on the interactable being dropped.
        /// </summary>
        /// <param name="grabInteractable">The XR Grab Interactable being dropped.</param>
        /// <param name="args">The event args associated with the select exit event.</param>
        /// <remarks>
        /// In other words, this will be called when the selection count changes from <c>1</c> to <c>0</c>.
        /// <br />
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="IXRGrabTransformer.OnGrab"/>
        /// <seealso cref="XRGrabInteractable.Drop"/>
        void OnDrop(XRGrabInteractable grabInteractable, DropEventArgs args);
    }
}
