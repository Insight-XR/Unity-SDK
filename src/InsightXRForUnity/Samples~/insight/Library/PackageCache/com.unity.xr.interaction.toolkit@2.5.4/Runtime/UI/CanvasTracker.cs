namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    /// <summary>
    /// Helper object for the <see cref="CanvasOptimizer"/>. 
    /// Monitors for hierarchy changes to ensure that only a top-level canvas is in place.
    /// </summary>
    [AddComponentMenu("")]
    [HelpURL(XRHelpURLConstants.k_CanvasTracker)]
    public class CanvasTracker : MonoBehaviour
    {
        /// <summary>
        /// Keeps track of if this canvas' place in the transform hierarchy has changed at all (parent added/removed, etc.).
        /// </summary>
        public bool transformDirty { get; set; }

        void OnEnable()
        {
            transformDirty = true;
        }

        void OnTransformParentChanged()
        {
            transformDirty = true;
        }
    }
}
