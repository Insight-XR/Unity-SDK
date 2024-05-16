namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The simplest Interactable object which provides events for interaction states like hover and select.
    /// </summary>
    /// <remarks>
    /// `XRSimpleInteractable` provides a concrete implementation of the <see cref="XRBaseInteractable"/>.
    /// A GameObject with this component responds to <see cref="XRBaseInteractable.hoverEntered"/>/<see cref="XRBaseInteractable.hoverExited"/>
    /// and <see cref="XRBaseInteractable.selectEntered"/>/<see cref="XRBaseInteractable.selectExited"/>
    /// events, but provides no default interaction behavior.
    ///
    /// For more information refer to:
    /// * [XR Simple Interactable component](xref:xri-simple-interactable)
    /// * [Create a grab interactable](xref:xri-general-setup#create-grab-interactable)
    /// * [UI interaction setup](xref:xri-ui-setup)
    /// * [Interaction States](xref:xri-architecture#states)
    /// </remarks>
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/XR Simple Interactable", 11)]
    [HelpURL(XRHelpURLConstants.k_XRSimpleInteractable)]
    public class XRSimpleInteractable : XRBaseInteractable
    {
    }
}
