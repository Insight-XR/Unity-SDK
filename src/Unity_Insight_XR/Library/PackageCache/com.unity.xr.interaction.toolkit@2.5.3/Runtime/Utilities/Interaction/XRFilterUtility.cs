using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// Utility methods for hover and select filters.
    /// </summary>
    static class XRFilterUtility
    {
        /// <summary>
        /// Returns the processing result of the given hover filters using the given Interactor and Interactable as
        /// parameters.
        /// </summary>
        /// <param name="filters">The hover filters to process.</param>
        /// <param name="interactor">The Interactor to be validate by the hover filters.</param>
        /// <param name="interactable">The Interactable to be validate by the hover filters.</param>
        /// <returns>
        /// Returns <see langword="true"/> if all processed filters also return <see langword="true"/>, or if
        /// <see cref="filters"/> is empty. Otherwise, returns <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method will ensure that all changes are buffered when processing, the buffered changes are applied
        /// when the processing is finished.
        /// </remarks>
        public static bool Process(SmallRegistrationList<IXRHoverFilter> filters, IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            if (filters.registeredSnapshot.Count == 0)
                return true;

            var alreadyBufferingChanges = filters.bufferChanges;
            filters.bufferChanges = true;
            var result = true;
            try
            {
                foreach (var filter in filters.registeredSnapshot)
                {
                    if (!filter.canProcess || filter.Process(interactor, interactable))
                        continue;

                    result = false;
                    break;
                }
            }
            finally
            {
                if (!alreadyBufferingChanges)
                    filters.bufferChanges = false;
            }

            return result;
        }

        /// <summary>
        /// Returns the processing result of the given select filters using the given Interactor and Interactable as
        /// parameters.
        /// </summary>
        /// <param name="filters">The select filters to process.</param>
        /// <param name="interactor">The Interactor to be validate by the select filters.</param>
        /// <param name="interactable">The Interactable to be validate by the select filters.</param>
        /// <returns>
        /// Returns <see langword="true"/> if all processed filters also return <see langword="true"/>, or if
        /// <see cref="filters"/> is empty. Otherwise, returns <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method will ensure that all changes are buffered when processing, the buffered changes are applied
        /// when the processing is finished.
        /// </remarks>
        public static bool Process(SmallRegistrationList<IXRSelectFilter> filters, IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            if (filters.registeredSnapshot.Count == 0)
                return true;

            var alreadyBufferingChanges = filters.bufferChanges;
            filters.bufferChanges = true;
            var result = true;
            try
            {
                foreach (var filter in filters.registeredSnapshot)
                {
                    if (!filter.canProcess || filter.Process(interactor, interactable))
                        continue;

                    result = false;
                    break;
                }
            }
            finally
            {
                if (!alreadyBufferingChanges)
                    filters.bufferChanges = false;
            }

            return result;
        }

        /// <summary>
        /// Returns the processing result of the given interaction strength filters using the given Interactor and Interactable as
        /// parameters.
        /// </summary>
        /// <param name="filters">The interaction strength filters to process.</param>
        /// <param name="interactor">The Interactor to process by the interaction strength filters.</param>
        /// <param name="interactable">The Interactable to process by the interaction strength filters.</param>
        /// <param name="interactionStrength">The interaction strength before processing.</param>
        /// <returns>Returns the modified interaction strength that is the result of passing the interaction strength through each filter.</returns>
        /// <remarks>
        /// This method will ensure that all changes are buffered when processing, the buffered changes are applied
        /// when the processing is finished.
        /// </remarks>
        public static float Process(SmallRegistrationList<IXRInteractionStrengthFilter> filters, IXRInteractor interactor, IXRInteractable interactable, float interactionStrength)
        {
            if (filters.registeredSnapshot.Count == 0)
                return interactionStrength;

            var alreadyBufferingChanges = filters.bufferChanges;
            filters.bufferChanges = true;
            try
            {
                foreach (var filter in filters.registeredSnapshot)
                {
                    if (filter.canProcess)
                        interactionStrength = filter.Process(interactor, interactable, interactionStrength);
                }
            }
            finally
            {
                if (!alreadyBufferingChanges)
                    filters.bufferChanges = false;
            }

            return interactionStrength;
        }
    }
}
