using System;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Instances that implement this interface are called hover filters. Hover filters process additional validation checks
    /// after the base class hover validation checks are processed.
    /// Add a hover filter to the following objects to extend its hover validations:
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="XRInteractionManager"/>: to add a global hover filter used to validate all hover
    /// interactions in the manager.</description>
    /// </item>
    /// <item>
    /// <description><see cref="XRBaseInteractor"/>: to add an Interactor hover filter used to validate the hover
    /// interactions in the Interactor.</description>
    /// </item>
    /// <item>
    /// <description><see cref="XRBaseInteractable"/>: to add an Interactable hover filter used to validate the
    /// hover interactions in the Interactable.</description>
    /// </item>
    /// </list>
    /// </summary>
    /// <seealso cref="XRInteractionManager.startingHoverFilters"/>
    /// <seealso cref="XRInteractionManager.hoverFilters"/>
    /// <seealso cref="XRBaseInteractor.startingHoverFilters"/>
    /// <seealso cref="XRBaseInteractor.hoverFilters"/>
    /// <seealso cref="XRBaseInteractable.startingHoverFilters"/>
    /// <seealso cref="XRBaseInteractable.hoverFilters"/>
    /// <seealso cref="IXRSelectFilter"/>
    public interface IXRHoverFilter
    {
        /// <summary>
        /// Whether this hover filter can process interactions.
        /// Hover filters that can process interactions receive calls to <see cref="Process"/>, hover filters that
        /// cannot process do not.
        /// </summary>
        /// <remarks>
        /// It's recommended to return <see cref="Behaviour.isActiveAndEnabled"/> when implementing this interface
        /// in a <see cref="MonoBehaviour"/>.
        /// </remarks>
        bool canProcess { get; }

        /// <summary>
        /// Called by the host object (<see cref="XRInteractionManager"/>, <see cref="XRBaseInteractor"/> or
        /// <see cref="XRBaseInteractable"/>) to verify if the hover interaction between the given Interactor and
        /// Interactable can be performed.
        /// </summary>
        /// <param name="interactor">The Interactor to validate the hover interaction.</param>
        /// <param name="interactable">The Interactable to validate the hover interaction.</param>
        /// <returns>
        /// Returns <see langword="true"/> when the given Interactor can hover the given Interactable. Otherwise,
        /// returns <see langword="false"/>.
        /// </returns>
        bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable);
    }

    /// <summary>
    /// A hover filter that forwards its processing to a delegate (<see cref="delegateToProcess"/>).
    /// Useful to create custom filters by code without needing to create new classes.
    /// </summary>
    /// <seealso cref="XRSelectFilterDelegate"/>
    public sealed class XRHoverFilterDelegate : IXRHoverFilter
    {
        /// <summary>
        /// The delegate to be invoked when processing this filter.
        /// </summary>
        public Func<IXRHoverInteractor, IXRHoverInteractable, bool> delegateToProcess { get; set; }

        /// <inheritdoc />
        public bool canProcess { get; set; } = true;

        /// <summary>
        /// Creates a new hover filter delegate.
        /// </summary>
        /// <param name="delegateToProcess">The delegate to be invoked when processing this filter.</param>
        public XRHoverFilterDelegate(Func<IXRHoverInteractor, IXRHoverInteractable, bool> delegateToProcess)
        {
            if (delegateToProcess == null)
                throw new ArgumentException(nameof(delegateToProcess));

            this.delegateToProcess = delegateToProcess;
        }

        /// <inheritdoc />
        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            return delegateToProcess.Invoke(interactor, interactable);
        }
    }
}
