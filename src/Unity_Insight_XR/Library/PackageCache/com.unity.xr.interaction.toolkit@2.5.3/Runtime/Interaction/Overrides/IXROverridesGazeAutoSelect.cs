namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that represents an interactable that provides
    /// overrides of the default values for hover to select and auto deselect.
    /// </summary>
    /// <seealso cref="XRBaseInteractable"/>
    /// <seealso cref="XRGazeInteractor.GetHoverTimeToSelect"/>
    /// <seealso cref="XRGazeInteractor.GetTimeToAutoDeselect"/>
    public interface IXROverridesGazeAutoSelect
    {
        /// <summary>
        /// Enables this interactable to override the <see cref="XRRayInteractor.hoverTimeToSelect"/> on a <see cref="XRGazeInteractor"/>.
        /// </summary>
        /// <seealso cref="gazeTimeToSelect"/>
        /// <seealso cref="XRRayInteractor.hoverToSelect"/>
        public bool overrideGazeTimeToSelect { get; }

        /// <summary>
        /// Number of seconds for which an <see cref="XRGazeInteractor"/> must hover over this interactable to select it if <see cref="XRRayInteractor.hoverToSelect"/> is enabled.
        /// </summary>
        /// <seealso cref="overrideGazeTimeToSelect"/>
        /// <seealso cref="XRRayInteractor.hoverTimeToSelect"/>
        public float gazeTimeToSelect { get; }

        /// <summary>
        /// Enables this interactable to override the <see cref="XRRayInteractor.timeToAutoDeselect"/> on a <see cref="XRGazeInteractor"/>.
        /// </summary>
        /// <seealso cref="timeToAutoDeselectGaze"/>
        /// <seealso cref="XRRayInteractor.autoDeselect"/>
        public bool overrideTimeToAutoDeselectGaze { get; }

        /// <summary>
        /// Number of seconds that the interactable will remain selected by a <see cref="XRGazeInteractor"/> before being
        /// automatically deselected if <see cref="overrideTimeToAutoDeselectGaze"/> is true.
        /// </summary>
        /// <seealso cref="overrideTimeToAutoDeselectGaze"/>
        public float timeToAutoDeselectGaze { get; }
    }
}
