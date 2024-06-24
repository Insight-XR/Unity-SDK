#if AR_FOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that allows interactors to raycast against the AR environment.
    /// </summary>
    /// <seealso cref="XRRayInteractor"/>
    public interface IARInteractor
    {
        /// <summary>
        /// Gets the first AR ray cast hit, if any ray cast hits are available.
        /// </summary>
        /// <param name="raycastHit">When this method returns, contains the ray cast hit if available; otherwise, the default value.</param>
        /// <returns>Returns <see langword="true"/> if a hit occurred, implying the ray cast hit information is valid.
        /// Otherwise, returns <see langword="false"/>.</returns>
        bool TryGetCurrentARRaycastHit(out ARRaycastHit raycastHit);

        /// <summary>
        /// The types of AR trackables that this interactor will be able to raycast against.
        /// </summary>
        TrackableType trackableType { get;}

        /// <summary>
        /// Enables raycasts against AR environment trackables.
        /// </summary>
        bool enableARRaycasting { get;}
        
        /// <summary>
        /// Enables occlusion of AR raycast hits by 3D objects.
        /// </summary>
        bool occludeARHitsWith3DObjects { get;}

        /// <summary>
        ///  Enables occlusion of AR raycast hits by 2D objects such as UI.
        /// </summary>
        bool occludeARHitsWith2DObjects { get;}
    }
}
#endif
