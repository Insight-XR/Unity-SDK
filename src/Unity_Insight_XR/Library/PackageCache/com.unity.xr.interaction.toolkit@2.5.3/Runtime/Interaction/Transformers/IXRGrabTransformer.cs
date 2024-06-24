namespace UnityEngine.XR.Interaction.Toolkit.Transformers
{
    /// <summary>
    /// An interface that allows the target position, rotation, and scale of an <see cref="XRGrabInteractable"/> to be
    /// calculated. The <see cref="Process"/> method is responsible for calculating the pose that the interactable
    /// will move to and the scale it will be resized to. Implementers are only responsible for calculating the pose and scale,
    /// which is handled and applied by Unity in <see cref="XRGrabInteractable"/>.
    /// </summary>
    /// <remarks>
    /// Implementers are encouraged to derive from <see cref="XRBaseGrabTransformer"/> instead of this interface directly.
    /// However, advanced users can use this interface to totally override that behavior.
    /// </remarks>
    /// <seealso cref="XRBaseGrabTransformer"/>
    /// <seealso cref="XRGrabInteractable"/>
    /// <seealso cref="IXRDropTransformer"/>
    public interface IXRGrabTransformer
    {
        /// <summary>
        /// Whether this grab transformer can process targets.
        /// Transformers that can process targets receive calls to <see cref="Process"/>, transformers that cannot process do not.
        /// Transformers will still have other event methods called to allow for initialization on the frame the grab changes happens.
        /// </summary>
        bool canProcess { get; }

        /// <summary>
        /// Called by Unity when the given Interactable links to this grab transformer.
        /// Use this to do any code initialization for the given Interactable.
        /// </summary>
        /// <param name="grabInteractable">The XR Grab Interactable being linked to this transformer.</param>
        /// <seealso cref="OnUnlink"/>
        void OnLink(XRGrabInteractable grabInteractable);

        /// <summary>
        /// Called by Unity when the given Interactable is grabbed (in other words, when entering the Select state).
        /// This method won't be called again until the Interactable is released by every Interactor.
        /// Use this to do any code initialization based on the first Interactor that selects the Interactable.
        /// </summary>
        /// <param name="grabInteractable">The XR Grab Interactable being grabbed.</param>
        /// <remarks>
        /// In other words, this will be called when the selection count changes from <c>0</c> to <c>1</c>.
        /// </remarks>
        /// <seealso cref="OnGrabCountChanged"/>
        /// <seealso cref="XRGrabInteractable.Grab"/>
        void OnGrab(XRGrabInteractable grabInteractable);

        /// <summary>
        /// Called by Unity each time the number of selections changes for the given Interactable
        /// while grabbed by at least one Interactor, including when it is first grabbed.
        /// Use this to do any code initialization based on each Interactor currently selecting the Interactable,
        /// for example computing the initial distance between both Interactors grabbing the object.
        /// </summary>
        /// <param name="grabInteractable">The XR Grab Interactable being grabbed.</param>
        /// <param name="targetPose">The current target pose for the current frame.</param>
        /// <param name="localScale">The current target scale of the Interactable's transform relative to the GameObject's parent.</param>
        /// <remarks>
        /// There will always be at least one Interactor selecting the Interactable when this method is called.
        /// In other words, this will be called when the selection count changes from <c>0</c> to <c>1</c>
        /// and whenever it subsequently changes while still above <c>0</c>.
        /// This method is called by Unity right before <see cref="Process"/> if the selection count changed.
        /// <br />
        /// <example>
        /// To get the number of Interactors selecting the Interactable in your implementation method:
        /// <code>
        /// grabInteractable.interactorsSelecting.Count
        /// </code>
        /// </example>
        /// </remarks>
        void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale);

        /// <summary>
        /// Called by the linked Interactable to calculate the target pose and scale.
        /// Modify the value of <paramref name="targetPose"/> and/or <paramref name="localScale"/> (or neither).
        /// </summary>
        /// <param name="grabInteractable">The XR Grab Interactable to calculate the target pose and scale for.</param>
        /// <param name="updatePhase">The update phase this is called during.</param>
        /// <param name="targetPose">The target pose for the current frame.</param>
        /// <param name="localScale">The target scale of the Interactable's transform relative to the GameObject's parent.</param>
        /// <remarks>
        /// When there is more than one linked grab transformer that can process, the updated value of each <see langword="ref"/> parameter
        /// is passed to each in series according to its order in the list. You can utilize this by, for example,
        /// having the first grab transformer compute the target pose, and the second compute just the scale.
        /// <br />
        /// <example>
        /// If your transformer requires the use of two or more selections, you should first check
        /// for that condition in your implementation method:
        /// <code>
        /// if (grabInteractable.interactorsSelecting.Count &lt; 2) return;
        /// </code>
        /// </example>
        /// </remarks>
        /// <seealso cref="XRGrabInteractable.ProcessInteractable"/>
        /// <seealso cref="XRInteractionUpdateOrder.UpdatePhase"/>
        void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale);

        /// <summary>
        /// Called by Unity when the given Interactable unlinks from this grab transformer.
        /// Use this to do any code cleanup for the given Interactable.
        /// </summary>
        /// <param name="grabInteractable">The XR Grab Interactable being unlinked from this transformer.</param>
        /// <seealso cref="OnLink"/>
        void OnUnlink(XRGrabInteractable grabInteractable);
    }
}
