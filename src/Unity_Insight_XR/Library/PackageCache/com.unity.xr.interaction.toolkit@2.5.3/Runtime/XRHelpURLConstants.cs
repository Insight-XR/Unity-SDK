using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Audio;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Transformation;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.UI;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Audio;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AR;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Constants for <see cref="HelpURLAttribute"/> for XR Interaction Toolkit.
    /// </summary>
    static partial class XRHelpURLConstants
    {
        const string k_CurrentDocsVersion = "2.5";
        const string k_BaseApi = "https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@" + k_CurrentDocsVersion + "/api/";
        const string k_BaseNamespace = "UnityEngine.XR.Interaction.Toolkit.";

        /// <summary>
        /// Current documentation version for XR Interaction Toolkit API and Manual pages.
        /// </summary>
        internal static string currentDocsVersion => k_CurrentDocsVersion;

        /// <summary>
        /// Scripting API URL for <see cref="ActionBasedContinuousMoveProvider"/>.
        /// </summary>
        public const string k_ActionBasedContinuousMoveProvider = k_BaseApi + k_BaseNamespace + nameof(ActionBasedContinuousMoveProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ActionBasedContinuousTurnProvider"/>.
        /// </summary>
        public const string k_ActionBasedContinuousTurnProvider = k_BaseApi + k_BaseNamespace + nameof(ActionBasedContinuousTurnProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ActionBasedController"/>.
        /// </summary>
        public const string k_ActionBasedController = k_BaseApi + k_BaseNamespace + nameof(ActionBasedController) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ActionBasedSnapTurnProvider"/>.
        /// </summary>
        public const string k_ActionBasedSnapTurnProvider = k_BaseApi + k_BaseNamespace + nameof(ActionBasedSnapTurnProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="AudioAffordanceReceiver"/>.
        /// </summary>
        public const string k_AudioAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Audio." + nameof(AudioAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ColorAffordanceReceiver"/>.
        /// </summary>
        public const string k_ColorAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Primitives." + nameof(ColorAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="FloatAffordanceReceiver"/>.
        /// </summary>
        public const string k_FloatAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Primitives." + nameof(FloatAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="QuaternionAffordanceReceiver"/>.
        /// </summary>
        public const string k_QuaternionAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Primitives." + nameof(QuaternionAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="QuaternionEulerAffordanceReceiver"/>.
        /// </summary>
        public const string k_QuaternionEulerAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Primitives." + nameof(QuaternionEulerAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="Vector2AffordanceReceiver"/>.
        /// </summary>
        public const string k_Vector2AffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Primitives." + nameof(Vector2AffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="Vector3AffordanceReceiver"/>.
        /// </summary>
        public const string k_Vector3AffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Primitives." + nameof(Vector3AffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="Vector4AffordanceReceiver"/>.
        /// </summary>
        public const string k_Vector4AffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Primitives." + nameof(Vector4AffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="BlendShapeAffordanceReceiver"/>.
        /// </summary>
        public const string k_BlendShapeAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Rendering." + nameof(BlendShapeAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ColorGradientLineRendererAffordanceReceiver"/>.
        /// </summary>
        public const string k_ColorGradientLineRendererAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Rendering." + nameof(ColorGradientLineRendererAffordanceReceiver) + ".html";
        
        /// <summary>
        /// Scripting API URL for <see cref="ColorMaterialPropertyAffordanceReceiver"/>.
        /// </summary>
        public const string k_ColorMaterialPropertyAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Rendering." + nameof(ColorMaterialPropertyAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="FloatMaterialPropertyAffordanceReceiver"/>.
        /// </summary>
        public const string k_FloatMaterialPropertyAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Rendering." + nameof(FloatMaterialPropertyAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="Vector2MaterialPropertyAffordanceReceiver"/>.
        /// </summary>
        public const string k_Vector2MaterialPropertyAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Rendering." + nameof(Vector2MaterialPropertyAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="Vector3MaterialPropertyAffordanceReceiver"/>.
        /// </summary>
        public const string k_Vector3MaterialPropertyAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Rendering." + nameof(Vector3MaterialPropertyAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="Vector4MaterialPropertyAffordanceReceiver"/>.
        /// </summary>
        public const string k_Vector4MaterialPropertyAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Rendering." + nameof(Vector4MaterialPropertyAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ImageColorAffordanceReceiver"/>.
        /// </summary>
        public const string k_ImageColorAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.UI." + nameof(ImageColorAffordanceReceiver) + ".html";
        
        /// <summary>
        /// Scripting API URL for <see cref="UniformTransformScaleAffordanceReceiver"/>.
        /// </summary>
        public const string k_UniformTransformScaleAffordanceReceiver = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Receiver.Transformation." + nameof(UniformTransformScaleAffordanceReceiver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="MaterialInstanceHelper"/>.
        /// </summary>
        public const string k_MaterialInstanceHelper = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Rendering." + nameof(MaterialInstanceHelper) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="MaterialPropertyBlockHelper"/>.
        /// </summary>
        public const string k_MaterialPropertyBlockHelper = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Rendering." + nameof(MaterialPropertyBlockHelper) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRInteractableAffordanceStateProvider"/>.
        /// </summary>
        public const string k_XRInteractableAffordanceStateProvider = k_BaseApi + k_BaseNamespace + "AffordanceSystem.State." + nameof(XRInteractableAffordanceStateProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRInteractorAffordanceStateProvider"/>.
        /// </summary>
        public const string k_XRInteractorAffordanceStateProvider = k_BaseApi + k_BaseNamespace + "AffordanceSystem.State." + nameof(XRInteractorAffordanceStateProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="AudioAffordanceThemeDatum"/>.
        /// </summary>
        public const string k_AudioAffordanceThemeDatum = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Theme.Audio." + nameof(AudioAffordanceThemeDatum) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ColorAffordanceThemeDatum"/>.
        /// </summary>
        public const string k_ColorAffordanceThemeDatum = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Theme.Primitives." + nameof(ColorAffordanceThemeDatum) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="FloatAffordanceThemeDatum"/>.
        /// </summary>
        public const string k_FloatAffordanceThemeDatum = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Theme.Primitives." + nameof(FloatAffordanceThemeDatum) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="Vector2AffordanceThemeDatum"/>.
        /// </summary>
        public const string k_Vector2AffordanceThemeDatum = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Theme.Primitives." + nameof(Vector2AffordanceThemeDatum) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="Vector3AffordanceThemeDatum"/>.
        /// </summary>
        public const string k_Vector3AffordanceThemeDatum = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Theme.Primitives." + nameof(Vector3AffordanceThemeDatum) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="Vector4AffordanceThemeDatum"/>.
        /// </summary>
        public const string k_Vector4AffordanceThemeDatum = k_BaseApi + k_BaseNamespace + "AffordanceSystem.Theme.Primitives." + nameof(Vector4AffordanceThemeDatum) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ARAnnotationInteractable"/>.
        /// </summary>
        public const string k_ARAnnotationInteractable = k_BaseApi + k_BaseNamespace + "AR." + nameof(ARAnnotationInteractable) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ARGestureInteractor"/>.
        /// </summary>
        public const string k_ARGestureInteractor = k_BaseApi + k_BaseNamespace + "AR." + nameof(ARGestureInteractor) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ARPlacementInteractable"/>.
        /// </summary>
        public const string k_ARPlacementInteractable = k_BaseApi + k_BaseNamespace + "AR." + nameof(ARPlacementInteractable) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ARRotationInteractable"/>.
        /// </summary>
        public const string k_ARRotationInteractable = k_BaseApi + k_BaseNamespace + "AR." + nameof(ARRotationInteractable) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ARScaleInteractable"/>.
        /// </summary>
        public const string k_ARScaleInteractable = k_BaseApi + k_BaseNamespace + "AR." + nameof(ARScaleInteractable) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ARSelectionInteractable"/>.
        /// </summary>
        public const string k_ARSelectionInteractable = k_BaseApi + k_BaseNamespace + "AR." + nameof(ARSelectionInteractable) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ARTranslationInteractable"/>.
        /// </summary>
        public const string k_ARTranslationInteractable = k_BaseApi + k_BaseNamespace + "AR." + nameof(ARTranslationInteractable) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="CharacterControllerDriver"/>.
        /// </summary>
        public const string k_CharacterControllerDriver = k_BaseApi + k_BaseNamespace + nameof(CharacterControllerDriver) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ClimbInteractable"/>.
        /// </summary>
        public const string k_ClimbInteractable = k_BaseApi + k_BaseNamespace + nameof(ClimbInteractable) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ClimbProvider"/>.
        /// </summary>
        public const string k_ClimbProvider = k_BaseApi + k_BaseNamespace + nameof(ClimbProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="ClimbSettingsDatum"/>.
        /// </summary>
        public const string k_ClimbSettingsDatum = k_BaseApi + k_BaseNamespace + nameof(ClimbSettingsDatum) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="DeviceBasedContinuousMoveProvider"/>.
        /// </summary>
        public const string k_DeviceBasedContinuousMoveProvider = k_BaseApi + k_BaseNamespace + nameof(DeviceBasedContinuousMoveProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="DeviceBasedContinuousTurnProvider"/>.
        /// </summary>
        public const string k_DeviceBasedContinuousTurnProvider = k_BaseApi + k_BaseNamespace + nameof(DeviceBasedContinuousTurnProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="DeviceBasedSnapTurnProvider"/>.
        /// </summary>
        public const string k_DeviceBasedSnapTurnProvider = k_BaseApi + k_BaseNamespace + nameof(DeviceBasedSnapTurnProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="PokeThresholdDatum"/>.
        /// </summary>
        public const string k_PokeThresholdDatum = k_BaseApi + k_BaseNamespace + "Filtering." + nameof(PokeThresholdDatum) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRPokeFilter"/>.
        /// </summary>
        public const string k_XRPokeFilter = k_BaseApi + k_BaseNamespace + "Filtering." + nameof(XRPokeFilter) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRTargetFilter"/>.
        /// </summary>
        public const string k_XRTargetFilter = k_BaseApi + k_BaseNamespace + "Filtering." + nameof(XRTargetFilter) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="GrabMoveProvider"/>.
        /// </summary>
        public const string k_GrabMoveProvider = k_BaseApi + k_BaseNamespace + nameof(GrabMoveProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="InputActionManager"/>.
        /// </summary>
        public const string k_InputActionManager = k_BaseApi + k_BaseNamespace + "Inputs." + nameof(InputActionManager) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRDeviceSimulator"/>.
        /// </summary>
        public const string k_XRDeviceSimulator = k_BaseApi + k_BaseNamespace + "Inputs.Simulation." + nameof(XRDeviceSimulator) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRHandSkeletonPokeDisplacer"/>.
        /// </summary>
        public const string k_XRHandSkeletonPokeDisplacer = k_BaseApi + k_BaseNamespace + "Inputs." + nameof(XRHandSkeletonPokeDisplacer) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRInputModalityManager"/>.
        /// </summary>
        public const string k_XRInputModalityManager = k_BaseApi + k_BaseNamespace + "Inputs." + nameof(XRInputModalityManager) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRTransformStabilizer"/>.
        /// </summary>
        public const string k_XRTransformStabilizer = k_BaseApi + k_BaseNamespace + "Inputs." + nameof(XRTransformStabilizer) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="LocomotionSystem"/>.
        /// </summary>
        public const string k_LocomotionSystem = k_BaseApi + k_BaseNamespace + nameof(LocomotionSystem) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="TeleportationAnchor"/>.
        /// </summary>
        public const string k_TeleportationAnchor = k_BaseApi + k_BaseNamespace + nameof(TeleportationAnchor) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="TeleportationArea"/>.
        /// </summary>
        public const string k_TeleportationArea = k_BaseApi + k_BaseNamespace + nameof(TeleportationArea) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="TeleportationProvider"/>.
        /// </summary>
        public const string k_TeleportationProvider = k_BaseApi + k_BaseNamespace + nameof(TeleportationProvider) + ".html";

#if AR_FOUNDATION_PRESENT || PACKAGE_DOCS_GENERATION
        /// <summary>
        /// Scripting API URL for <see cref="ARTransformer"/>.
        /// </summary>
        public const string k_ARTransformer = k_BaseApi + k_BaseNamespace + "Transformers." + nameof(ARTransformer) + ".html";
#endif

        /// <summary>
        /// Scripting API URL for <see cref="XRDualGrabFreeTransformer"/>.
        /// </summary>
        public const string k_XRDualGrabFreeTransformer = k_BaseApi + k_BaseNamespace + "Transformers." + nameof(XRDualGrabFreeTransformer) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRGeneralGrabTransformer"/>.
        /// </summary>
        public const string k_XRGeneralGrabTransformer = k_BaseApi + k_BaseNamespace + "Transformers." + nameof(XRGeneralGrabTransformer) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRLegacyGrabTransformer"/>.
        /// </summary>
#pragma warning disable 618 // Deprecated component for compatibility with existing user projects.
        public const string k_XRLegacyGrabTransformer = k_BaseApi + k_BaseNamespace + "Transformers." + nameof(XRLegacyGrabTransformer) + ".html";
#pragma warning restore 618

        /// <summary>
        /// Scripting API URL for <see cref="XRSingleGrabFreeTransformer"/>.
        /// </summary>
        public const string k_XRSingleGrabFreeTransformer = k_BaseApi + k_BaseNamespace + "Transformers." + nameof(XRSingleGrabFreeTransformer) + ".html";
        
        /// <summary>
        /// Scripting API URL for <see cref="XRSingleGrabOffsetPreserveTransformer"/>.
        /// </summary>
        public const string k_XRSingleGrabOffsetPreserveTransformer = k_BaseApi + k_BaseNamespace + "Transformers." + nameof(k_XRSingleGrabOffsetPreserveTransformer) + ".html";
        
        /// <summary>
        /// Scripting API URL for <see cref="XRSocketGrabTransformer"/>.
        /// </summary>
        public const string k_XRSocketGrabTransformer = k_BaseApi + k_BaseNamespace + "Transformers." + nameof(XRSocketGrabTransformer) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="TunnelingVignetteController"/>
        /// </summary>
        public const string k_TunnelingVignetteController = k_BaseApi + k_BaseNamespace + nameof(TunnelingVignetteController) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="TwoHandedGrabMoveProvider"/>.
        /// </summary>
        public const string k_TwoHandedGrabMoveProvider = k_BaseApi + k_BaseNamespace + nameof(TwoHandedGrabMoveProvider) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="HandMenu"/>.
        /// </summary>
        public const string k_HandMenu = k_BaseApi + k_BaseNamespace + "UI.BodyUI." + nameof(HandMenu) + ".html";
        
        /// <summary>
        /// Scripting API URL for <see cref="FollowPresetDatum"/>.
        /// </summary>
        public const string k_FollowPresetDatum = k_BaseApi + k_BaseNamespace + "UI.BodyUI." + nameof(FollowPresetDatum) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="CanvasOptimizer"/>.
        /// </summary>
        public const string k_CanvasOptimizer = k_BaseApi + k_BaseNamespace + "UI." + nameof(CanvasOptimizer) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="CanvasTracker"/>.
        /// </summary>
        public const string k_CanvasTracker = k_BaseApi + k_BaseNamespace + "UI." + nameof(CanvasTracker) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="LazyFollow"/>.
        /// </summary>
        public const string k_LazyFollow = k_BaseApi + k_BaseNamespace + "UI." + nameof(LazyFollow) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="TrackedDeviceGraphicRaycaster"/>.
        /// </summary>
        public const string k_TrackedDeviceGraphicRaycaster = k_BaseApi + k_BaseNamespace + "UI." + nameof(TrackedDeviceGraphicRaycaster) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="TrackedDevicePhysicsRaycaster"/>
        /// </summary>
        public const string k_TrackedDevicePhysicsRaycaster = k_BaseApi + k_BaseNamespace + "UI." + nameof(TrackedDevicePhysicsRaycaster) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRUIInputModule"/>.
        /// </summary>
        public const string k_XRUIInputModule = k_BaseApi + k_BaseNamespace + "UI." + nameof(XRUIInputModule) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="DisposableManagerSingleton"/>.
        /// </summary>
        public const string k_DisposableManagerSingleton = k_BaseApi + k_BaseNamespace + "Utilities." + nameof(DisposableManagerSingleton) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRController"/>.
        /// </summary>
        public const string k_XRController = k_BaseApi + k_BaseNamespace + nameof(XRController) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRControllerRecorder"/>.
        /// </summary>
        public const string k_XRControllerRecorder = k_BaseApi + k_BaseNamespace + nameof(XRControllerRecorder) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRControllerRecording"/>.
        /// </summary>
        public const string k_XRControllerRecording = k_BaseApi + k_BaseNamespace + nameof(XRControllerRecording) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRDirectInteractor"/>.
        /// </summary>
        public const string k_XRDirectInteractor = k_BaseApi + k_BaseNamespace + nameof(XRDirectInteractor) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRGazeAssistance"/>.
        /// </summary>
        public const string k_XRGazeAssistance = k_BaseApi + k_BaseNamespace + nameof(XRGazeAssistance) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRGazeInteractor"/>.
        /// </summary>
        public const string k_XRGazeInteractor = k_BaseApi + k_BaseNamespace + nameof(XRGazeInteractor) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRGrabInteractable"/>.
        /// </summary>
        public const string k_XRGrabInteractable = k_BaseApi + k_BaseNamespace + nameof(XRGrabInteractable) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRInteractableSnapVolume"/>.
        /// </summary>
        public const string k_XRInteractableSnapVolume = k_BaseApi + k_BaseNamespace + nameof(XRInteractableSnapVolume) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRInteractionGroup"/>.
        /// </summary>
        public const string k_XRInteractionGroup = k_BaseApi + k_BaseNamespace + nameof(XRInteractionGroup) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRInteractionManager"/>.
        /// </summary>
        public const string k_XRInteractionManager = k_BaseApi + k_BaseNamespace + nameof(XRInteractionManager) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRInteractorLineVisual"/>.
        /// </summary>
        public const string k_XRInteractorLineVisual = k_BaseApi + k_BaseNamespace + nameof(XRInteractorLineVisual) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRInteractorReticleVisual"/>.
        /// </summary>
        public const string k_XRInteractorReticleVisual = k_BaseApi + k_BaseNamespace + nameof(XRInteractorReticleVisual) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRPokeInteractor"/>.
        /// </summary>
        public const string k_XRPokeInteractor = k_BaseApi + k_BaseNamespace + nameof(XRPokeInteractor) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRRayInteractor"/>.
        /// </summary>
        public const string k_XRRayInteractor = k_BaseApi + k_BaseNamespace + nameof(XRRayInteractor) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRScreenSpaceController"/>.
        /// </summary>
        public const string k_XRScreenSpaceController = k_BaseApi + k_BaseNamespace + nameof(XRScreenSpaceController) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRSimpleInteractable"/>.
        /// </summary>
        public const string k_XRSimpleInteractable = k_BaseApi + k_BaseNamespace + nameof(XRSimpleInteractable) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRSocketInteractor"/>.
        /// </summary>
        public const string k_XRSocketInteractor = k_BaseApi + k_BaseNamespace + nameof(XRSocketInteractor) + ".html";

        /// <summary>
        /// Scripting API URL for <see cref="XRTintInteractableVisual"/>.
        /// </summary>
        public const string k_XRTintInteractableVisual = k_BaseApi + k_BaseNamespace + nameof(XRTintInteractableVisual) + ".html";
    }
}
