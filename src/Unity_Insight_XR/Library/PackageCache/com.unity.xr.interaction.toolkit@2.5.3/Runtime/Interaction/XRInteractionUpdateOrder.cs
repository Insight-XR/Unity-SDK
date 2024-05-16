using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The update order for <see cref="MonoBehaviour"/> types in XR Interaction Toolkit.
    /// </summary>
    /// <remarks>
    /// This is primarily used to control initialization order as the update of interactors / interaction manager / interactables is handled by the
    /// Interaction managers themselves.
    /// </remarks>
    public static class XRInteractionUpdateOrder
    {
        /// <summary>
        /// Order when instances of type <see cref="XRControllerRecorder"/> are updated.
        /// </summary>
        public const int k_ControllerRecorder = -30000;

        /// <summary>
        /// Order when instances of type <see cref="XRDeviceSimulator"/> are updated.
        /// </summary>
        public const int k_DeviceSimulator = -29991; // Before XRBaseController

        /// <summary>
        /// Order when instances of type <see cref="XRBaseController"/> are updated.
        /// </summary>
        public const int k_Controllers = -29990; // After XRControllerRecorder

        /// <summary>
        /// Order when instances of type <see cref="XRTransformStabilizer"/> are updated.
        /// </summary>
        public const int k_TransformStabilizer = -29985; // After Controllers apply pose to Transforms

        /// <summary>
        /// Order when instances of type <see cref="XRGazeAssistance"/> are updated.
        /// </summary>
        public const int k_GazeAssistance = -29980; // After input pose stabilization

        /// <summary>
        /// Order when instances of type <see cref="LocomotionProvider"/> are updated.
        /// </summary>
        public const int k_LocomotionProviders = -210; // Before UIInputModule

        /// <summary>
        /// Order when instances of type <see cref="TwoHandedGrabMoveProvider"/> are updated.
        /// </summary>
        public const int k_TwoHandedGrabMoveProviders = -209; // After GrabMoveProvider

        /// <summary>
        /// Order when instances of type <see cref="UIInputModule"/> are updated.
        /// </summary>
        public const int k_UIInputModule = -200;

        /// <summary>
        /// Order when <see cref="XRInteractionManager"/> is updated.
        /// </summary>
        public const int k_InteractionManager = -105;

        /// <summary>
        /// Order when instances of type <see cref="XRInteractionGroup"/> are updated.
        /// </summary>
        public const int k_InteractionGroups = -100; // After XRInteractionManager

        /// <summary>
        /// Order when instances of type <see cref="XRBaseInteractor"/> are updated.
        /// </summary>
        public const int k_Interactors = -99; // After XRInteractionGroup

        /// <summary>
        /// Order when instances of type <see cref="XRInteractableSnapVolume"/> are updated.
        /// </summary>
        /// <remarks>
        /// Executes before interactables to ensure colliders have been found and set to trigger colliders
        /// so they are filtered out in the <see cref="XRBaseInteractable"/>.
        /// </remarks>
        public const int k_InteractableSnapVolume = -99; // Before XRBaseInteractable

        /// <summary>
        /// Order when instances of type <see cref="XRBaseInteractable"/> are updated.
        /// </summary>
        public const int k_Interactables = -98; // After XRBaseInteractor

        /// <summary>
        /// Order when instances of type <see cref="XRInteractorLineVisual"/> are updated.
        /// </summary>
        public const int k_LineVisual = 100;

        /// <summary>
        /// Order when <see cref="XRGazeAssistance.OnBeforeRender"/> is called.
        /// </summary>
        public const int k_BeforeRenderGazeAssistance = 95;

        /// <summary>
        /// Order when <see cref="XRInteractionManager.OnBeforeRender"/> is called.
        /// </summary>
        public const int k_BeforeRenderOrder = 100;

        /// <summary>
        /// Order when <see cref="XRInteractorLineVisual.OnBeforeRenderLineVisual"/> is called.
        /// </summary>
        public const int k_BeforeRenderLineVisual = 101; // After XRInteractionManager.OnBeforeRender

        /// <summary>
        /// The phase in which updates happen.
        /// </summary>
        /// <seealso cref="IXRInteractor.ProcessInteractor"/>
        /// <seealso cref="IXRInteractable.ProcessInteractable"/>
        public enum UpdatePhase
        {
            /// <summary>
            /// Frame-rate independent. Corresponds with the <c>MonoBehaviour.FixedUpdate</c> method.
            /// </summary>
            Fixed,

            /// <summary>
            /// Called every frame. Corresponds with the <c>MonoBehaviour.Update</c> method.
            /// </summary>
            Dynamic,

            /// <summary>
            /// Called at the end of every frame.  Corresponds with the <c>MonoBehaviour.LateUpdate</c> method.
            /// </summary>
            Late,

            /// <summary>
            /// Called just before render. Corresponds with the <see cref="Application.onBeforeRender"/> callback.
            /// </summary>
            OnBeforeRender,
        }
    }
}
