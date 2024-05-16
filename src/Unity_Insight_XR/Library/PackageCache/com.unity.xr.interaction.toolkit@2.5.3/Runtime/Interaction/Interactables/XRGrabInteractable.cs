using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;
#if BURST_PRESENT
using Unity.Burst;
#endif

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactable component that allows for basic grab functionality.
    /// When this behavior is selected (grabbed) by an Interactor, this behavior will follow it around
    /// and inherit velocity when released.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This behavior is responsible for applying the position, rotation, and local scale calculated
    /// by one or more <see cref="IXRGrabTransformer"/> implementations. A default set of grab transformers
    /// are automatically added by Unity, but this functionality can be disabled to manually set those
    /// used by this behavior, allowing you to customize where this component should move and rotate to.
    /// </para>
    /// <para>
    /// Grab transformers are classified into two different types: Single and Multiple.
    /// Those added to the Single Grab Transformers list are used when there is a single interactor selecting this object.
    /// Those added to the Multiple Grab Transformers list are used when there are multiple interactors selecting this object.
    /// You can add multiple grab transformers in a category and they will each be processed in sequence.
    /// The Multiple Grab Transformers are given first opportunity to process when there are multiple grabs, and
    /// the Single Grab Transformer processing will be skipped if a Multiple Grab Transformer can process in that case.
    /// </para>
    /// <para>
    /// There are fallback rules that could allow a Single Grab Transformer to be processed when there are multiple grabs,
    /// and for a Multiple Grab Transformer to be processed when there is a single grab (though a grab transformer will never be
    /// processed if its <see cref="IXRGrabTransformer.canProcess"/> returns <see langword="false"/>).
    /// <list type="bullet">
    /// <item>
    /// <description>When there is a single interactor selecting this object, the Multiple Grab Transformer will be processed only
    ///  if the Single Grab Transformer list is empty or if all transformers in the Single Grab Transformer list return false during processing.</description>
    /// </item>
    /// <item>
    /// <description>When there are multiple interactors selecting this object, the Single Grab Transformer will be processed only
    /// if the Multiple Grab Transformer list is empty or if all transformers in the Multiple Grab Transformer list return false during processing.</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="IXRGrabTransformer"/>
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("XR/XR Grab Interactable", 11)]
    [HelpURL(XRHelpURLConstants.k_XRGrabInteractable)]
#if BURST_PRESENT
    [BurstCompile]
#endif
    public partial class XRGrabInteractable : XRBaseInteractable
    {
        const float k_DefaultTighteningAmount = 0.1f;
        const float k_DefaultSmoothingAmount = 8f;
        const float k_LinearVelocityDamping = 1f;
        const float k_LinearVelocityScale = 1f;
        const float k_AngularVelocityDamping = 1f;
        const float k_AngularVelocityScale = 1f;
        const int k_ThrowSmoothingFrameCount = 20;
        const float k_DefaultAttachEaseInTime = 0.15f;
        const float k_DefaultThrowSmoothingDuration = 0.25f;
        const float k_DefaultThrowLinearVelocityScale = 1.5f;
        const float k_DefaultThrowAngularVelocityScale = 1f;
        const float k_DeltaTimeThreshold = 0.001f;

        /// <summary>
        /// Controls the method used when calculating the target position of the object.
        /// </summary>
        /// <seealso cref="attachPointCompatibilityMode"/>
        public enum AttachPointCompatibilityMode
        {
            /// <summary>
            /// Use the default, correct method for calculating the target position of the object.
            /// </summary>
            Default,

            /// <summary>
            /// Use an additional offset from the center of mass when calculating the target position of the object.
            /// Also incorporate the scale of the Interactor's Attach Transform.
            /// Marked for deprecation.
            /// This is the backwards compatible support mode for projects that accounted for the
            /// unintended difference when using XR Interaction Toolkit prior to version <c>1.0.0-pre.4</c>.
            /// To have the effective attach position be the same between all <see cref="XRBaseInteractable.MovementType"/> values, use <see cref="Default"/>.
            /// </summary>
            Legacy,
        }

        [SerializeField]
        Transform m_AttachTransform;

        /// <summary>
        /// The attachment point Unity uses on this Interactable (will use this object's position if none set).
        /// </summary>
        public Transform attachTransform
        {
            get => m_AttachTransform;
            set => m_AttachTransform = value;
        }

        [SerializeField]
        Transform m_SecondaryAttachTransform;

        /// <summary>
        /// The secondary attachment point to use on this Interactable for multi-hand interaction (will use the second interactor's attach transform if none set).
        /// Used for multi-grab interactions.
        /// </summary>
        public Transform secondaryAttachTransform
        {
            get => m_SecondaryAttachTransform;
            set => m_SecondaryAttachTransform = value;
        }

        [SerializeField]
        bool m_UseDynamicAttach;

        /// <summary>
        /// The grab pose will be based on the pose of the Interactor when the selection is made.
        /// Unity will create a dynamic attachment point for each Interactor that selects this component.
        /// </summary>
        /// <remarks>
        /// A child GameObject will be created for each Interactor that selects this component to serve as the attachment point.
        /// These are cached and part of a shared pool used by all instances of <see cref="XRGrabInteractable"/>.
        /// Therefore, while a reference can be obtained by calling <see cref="GetAttachTransform"/> while selected,
        /// you should typically not add any components to that GameObject unless you remove them after being released
        /// since it won't always be used by the same Interactable.
        /// </remarks>
        /// <seealso cref="attachTransform"/>
        /// <seealso cref="InitializeDynamicAttachPose"/>
        public bool useDynamicAttach
        {
            get => m_UseDynamicAttach;
            set => m_UseDynamicAttach = value;
        }

        [SerializeField]
        bool m_MatchAttachPosition = true;

        /// <summary>
        /// Match the position of the Interactor's attachment point when initializing the grab.
        /// This will override the position of <see cref="attachTransform"/>.
        /// </summary>
        /// <remarks>
        /// This will initialize the dynamic attachment point of this object using the position of the Interactor's attachment point.
        /// This value can be overridden for a specific interactor by overriding <see cref="ShouldMatchAttachPosition"/>.
        /// </remarks>
        /// <seealso cref="useDynamicAttach"/>
        /// <seealso cref="matchAttachRotation"/>
        public bool matchAttachPosition
        {
            get => m_MatchAttachPosition;
            set => m_MatchAttachPosition = value;
        }

        [SerializeField]
        bool m_MatchAttachRotation = true;

        /// <summary>
        /// Match the rotation of the Interactor's attachment point when initializing the grab.
        /// This will override the rotation of <see cref="attachTransform"/>.
        /// </summary>
        /// <remarks>
        /// This will initialize the dynamic attachment point of this object using the rotation of the Interactor's attachment point.
        /// This value can be overridden for a specific interactor by overriding <see cref="ShouldMatchAttachRotation"/>.
        /// </remarks>
        /// <seealso cref="useDynamicAttach"/>
        /// <seealso cref="matchAttachPosition"/>
        public bool matchAttachRotation
        {
            get => m_MatchAttachRotation;
            set => m_MatchAttachRotation = value;
        }

        [SerializeField]
        bool m_SnapToColliderVolume = true;

        /// <summary>
        /// Adjust the dynamic attachment point to keep it on or inside the Colliders that make up this object.
        /// </summary>
        /// <seealso cref="useDynamicAttach"/>
        /// <seealso cref="ShouldSnapToColliderVolume"/>
        /// <seealso cref="Collider.ClosestPoint"/>
        public bool snapToColliderVolume
        {
            get => m_SnapToColliderVolume;
            set => m_SnapToColliderVolume = value;
        }

        [SerializeField]
        bool m_ReinitializeDynamicAttachEverySingleGrab = true;

        /// <summary>
        /// Re-initialize the dynamic attachment pose when changing from multiple grabs back to a single grab.
        /// Use this if you want to keep the current pose of the object after releasing a second hand
        /// rather than reverting back to the attach pose from the original grab.
        /// </summary>
        /// <remarks>
        /// <see cref="IXRSelectInteractable.selectMode"/> must be set to <see cref="InteractableSelectMode.Multiple"/> for
        /// this setting to take effect.
        /// </remarks>
        /// <seealso cref="useDynamicAttach"/>
        /// <seealso cref="IXRSelectInteractable.selectMode"/>
        public bool reinitializeDynamicAttachEverySingleGrab
        {
            get => m_ReinitializeDynamicAttachEverySingleGrab;
            set => m_ReinitializeDynamicAttachEverySingleGrab = value;
        }

        [SerializeField]
        float m_AttachEaseInTime = k_DefaultAttachEaseInTime;

        /// <summary>
        /// Time in seconds Unity eases in the attach when selected (a value of 0 indicates no easing).
        /// </summary>
        public float attachEaseInTime
        {
            get => m_AttachEaseInTime;
            set => m_AttachEaseInTime = value;
        }

        [SerializeField]
        MovementType m_MovementType = MovementType.Instantaneous;

        /// <summary>
        /// Specifies how this object moves when selected, either through setting the velocity of the <see cref="Rigidbody"/>,
        /// moving the kinematic <see cref="Rigidbody"/> during Fixed Update, or by directly updating the <see cref="Transform"/> each frame.
        /// </summary>
        /// <seealso cref="XRBaseInteractable.MovementType"/>
        public MovementType movementType
        {
            get => m_MovementType;
            set
            {
                m_MovementType = value;

                if (isSelected)
                {
                    SetupRigidbodyDrop(m_Rigidbody);
                    UpdateCurrentMovementType();
                    SetupRigidbodyGrab(m_Rigidbody);
                }
            }
        }

        [SerializeField, Range(0f, 1f)]
        float m_VelocityDamping = k_LinearVelocityDamping;

        /// <summary>
        /// Scale factor of how much to dampen the existing linear velocity when tracking the position of the Interactor.
        /// The smaller the value, the longer it takes for the velocity to decay.
        /// </summary>
        /// <remarks>
        /// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
        /// </remarks>
        /// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
        /// <seealso cref="trackPosition"/>
        public float velocityDamping
        {
            get => m_VelocityDamping;
            set => m_VelocityDamping = value;
        }

        [SerializeField]
        float m_VelocityScale = k_LinearVelocityScale;

        /// <summary>
        /// Scale factor Unity applies to the tracked linear velocity while updating the <see cref="Rigidbody"/>
        /// when tracking the position of the Interactor.
        /// </summary>
        /// <remarks>
        /// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
        /// </remarks>
        /// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
        /// <seealso cref="trackPosition"/>
        public float velocityScale
        {
            get => m_VelocityScale;
            set => m_VelocityScale = value;
        }

        [SerializeField, Range(0f, 1f)]
        float m_AngularVelocityDamping = k_AngularVelocityDamping;

        /// <summary>
        /// Scale factor of how much Unity dampens the existing angular velocity when tracking the rotation of the Interactor.
        /// The smaller the value, the longer it takes for the angular velocity to decay.
        /// </summary>
        /// <remarks>
        /// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
        /// </remarks>
        /// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
        /// <seealso cref="trackRotation"/>
        public float angularVelocityDamping
        {
            get => m_AngularVelocityDamping;
            set => m_AngularVelocityDamping = value;
        }

        [SerializeField]
        float m_AngularVelocityScale = k_AngularVelocityScale;

        /// <summary>
        /// Scale factor Unity applies to the tracked angular velocity while updating the <see cref="Rigidbody"/>
        /// when tracking the rotation of the Interactor.
        /// </summary>
        /// <remarks>
        /// Only applies when in <see cref="XRBaseInteractable.MovementType.VelocityTracking"/> mode.
        /// </remarks>
        /// <seealso cref="XRBaseInteractable.MovementType.VelocityTracking"/>
        /// <seealso cref="trackRotation"/>
        public float angularVelocityScale
        {
            get => m_AngularVelocityScale;
            set => m_AngularVelocityScale = value;
        }

        [SerializeField]
        bool m_TrackPosition = true;

        /// <summary>
        /// Whether this object should follow the position of the Interactor when selected.
        /// </summary>
        /// <seealso cref="trackRotation"/>
        /// <seealso cref="trackScale"/>
        public bool trackPosition
        {
            get => m_TrackPosition;
            set => m_TrackPosition = value;
        }

        [SerializeField]
        bool m_SmoothPosition;

        /// <summary>
        /// Whether Unity applies smoothing while following the position of the Interactor when selected.
        /// </summary>
        /// <seealso cref="smoothPositionAmount"/>
        /// <seealso cref="tightenPosition"/>
        public bool smoothPosition
        {
            get => m_SmoothPosition;
            set => m_SmoothPosition = value;
        }

        [SerializeField, Range(0f, 20f)]
        float m_SmoothPositionAmount = k_DefaultSmoothingAmount;

        /// <summary>
        /// Scale factor for how much smoothing is applied while following the position of the Interactor when selected.
        /// The larger the value, the closer this object will remain to the position of the Interactor.
        /// </summary>
        /// <seealso cref="smoothPosition"/>
        /// <seealso cref="tightenPosition"/>
        public float smoothPositionAmount
        {
            get => m_SmoothPositionAmount;
            set => m_SmoothPositionAmount = value;
        }

        [SerializeField, Range(0f, 1f)]
        float m_TightenPosition = k_DefaultTighteningAmount;

        /// <summary>
        /// Reduces the maximum follow position difference when using smoothing.
        /// </summary>
        /// <remarks>
        /// Fractional amount of how close the smoothed position should remain to the position of the Interactor when using smoothing.
        /// The value ranges from 0 meaning no bias in the smoothed follow distance,
        /// to 1 meaning effectively no smoothing at all.
        /// </remarks>
        /// <seealso cref="smoothPosition"/>
        /// <seealso cref="smoothPositionAmount"/>
        public float tightenPosition
        {
            get => m_TightenPosition;
            set => m_TightenPosition = value;
        }

        [SerializeField]
        bool m_TrackRotation = true;

        /// <summary>
        /// Whether this object should follow the rotation of the Interactor when selected.
        /// </summary>
        /// <seealso cref="trackPosition"/>
        /// <seealso cref="trackScale"/>
        public bool trackRotation
        {
            get => m_TrackRotation;
            set => m_TrackRotation = value;
        }

        [SerializeField]
        bool m_SmoothRotation;

        /// <summary>
        /// Apply smoothing while following the rotation of the Interactor when selected.
        /// </summary>
        /// <seealso cref="smoothRotationAmount"/>
        /// <seealso cref="tightenRotation"/>
        public bool smoothRotation
        {
            get => m_SmoothRotation;
            set => m_SmoothRotation = value;
        }

        [SerializeField, Range(0f, 20f)]
        float m_SmoothRotationAmount = k_DefaultSmoothingAmount;

        /// <summary>
        /// Scale factor for how much smoothing is applied while following the rotation of the Interactor when selected.
        /// The larger the value, the closer this object will remain to the rotation of the Interactor.
        /// </summary>
        /// <seealso cref="smoothRotation"/>
        /// <seealso cref="tightenRotation"/>
        public float smoothRotationAmount
        {
            get => m_SmoothRotationAmount;
            set => m_SmoothRotationAmount = value;
        }

        [SerializeField, Range(0f, 1f)]
        float m_TightenRotation = k_DefaultTighteningAmount;

        /// <summary>
        /// Reduces the maximum follow rotation difference when using smoothing.
        /// </summary>
        /// <remarks>
        /// Fractional amount of how close the smoothed rotation should remain to the rotation of the Interactor when using smoothing.
        /// The value ranges from 0 meaning no bias in the smoothed follow rotation,
        /// to 1 meaning effectively no smoothing at all.
        /// </remarks>
        /// <seealso cref="smoothRotation"/>
        /// <seealso cref="smoothRotationAmount"/>
        public float tightenRotation
        {
            get => m_TightenRotation;
            set => m_TightenRotation = value;
        }

        [SerializeField]
        bool m_TrackScale = true;

        /// <summary>
        /// Whether or not the interactor will affect the scale of the object when selected.
        /// </summary>
        /// <seealso cref="trackPosition"/>
        /// <seealso cref="trackRotation"/>
        public bool trackScale
        {
            get => m_TrackScale;
            set => m_TrackScale = value;
        }

        [SerializeField]
        bool m_SmoothScale;

        /// <summary>
        /// Whether Unity applies smoothing while following the scale of the Interactor when selected.
        /// </summary>
        /// <seealso cref="smoothScaleAmount"/>
        /// <seealso cref="tightenScale"/>
        public bool smoothScale
        {
            get => m_SmoothScale;
            set => m_SmoothScale = value;
        }

        [SerializeField, Range(0f, 20f)]
        float m_SmoothScaleAmount = k_DefaultSmoothingAmount;

        /// <summary>
        /// Scale factor for how much smoothing is applied while following the scale of the Interactor when selected.
        /// The larger the value, the closer this object will remain to the scale of the Interactor.
        /// </summary>
        /// <seealso cref="smoothScale"/>
        /// <seealso cref="tightenScale"/>
        public float smoothScaleAmount
        {
            get => m_SmoothScaleAmount;
            set => m_SmoothScaleAmount = value;
        }

        [SerializeField, Range(0f, 1f)]
        float m_TightenScale = k_DefaultTighteningAmount;

        /// <summary>
        /// Reduces the maximum follow scale difference when using smoothing.
        /// </summary>
        /// <remarks>
        /// Scale factor for how much smoothing is applied while following the scale of the determined by the transformer when selected. The larger the value, the closer this object will remain to the target scale determined by the interactable's transformer.
        /// The value ranges from 0 meaning no bias in the smoothed follow distance,
        /// to 1 meaning effectively no smoothing at all.
        /// </remarks>
        /// <seealso cref="smoothScale"/>
        /// <seealso cref="smoothScaleAmount"/>
        public float tightenScale
        {
            get => m_TightenScale;
            set => m_TightenScale = value;
        }

        [SerializeField]
        bool m_ThrowOnDetach = true;

        /// <summary>
        /// Whether this object inherits the velocity of the Interactor when released.
        /// </summary>
        public bool throwOnDetach
        {
            get => m_ThrowOnDetach;
            set => m_ThrowOnDetach = value;
        }

        [SerializeField]
        float m_ThrowSmoothingDuration = k_DefaultThrowSmoothingDuration;

        /// <summary>
        /// This value represents the time over which collected samples are used for velocity calculation
        /// (up to a max of 20 previous frames, which is dependent on both Smoothing Duration and framerate).
        /// </summary>
        /// <remarks>
        /// As an example, if this value is set to 0.25, position and velocity values will be averaged over the past 0.25 seconds.
        /// Each of those values is weighted (multiplied) by the <see cref="throwSmoothingCurve"/> as well.</remarks>
        /// <seealso cref="throwSmoothingCurve"/>
        /// <seealso cref="throwOnDetach"/>
        public float throwSmoothingDuration
        {
            get => m_ThrowSmoothingDuration;
            set => m_ThrowSmoothingDuration = value;
        }

        [SerializeField]
        AnimationCurve m_ThrowSmoothingCurve = AnimationCurve.Linear(1f, 1f, 1f, 0f);

        /// <summary>
        /// The curve used to weight velocity smoothing upon throwing (most recent frames to the right).
        /// </summary>
        /// <remarks>
        /// By default this curve is flat with a 1.0 value so all smoothing values are treated equally across the smoothing duration.
        /// </remarks>
        /// <seealso cref="throwSmoothingDuration"/>
        /// <seealso cref="throwOnDetach"/>
        public AnimationCurve throwSmoothingCurve
        {
            get => m_ThrowSmoothingCurve;
            set => m_ThrowSmoothingCurve = value;
        }

        [SerializeField]
        float m_ThrowVelocityScale = k_DefaultThrowLinearVelocityScale;

        /// <summary>
        /// Scale factor Unity applies to this object's linear velocity inherited from the Interactor when released.
        /// </summary>
        /// <seealso cref="throwOnDetach"/>
        public float throwVelocityScale
        {
            get => m_ThrowVelocityScale;
            set => m_ThrowVelocityScale = value;
        }

        [SerializeField]
        float m_ThrowAngularVelocityScale = k_DefaultThrowAngularVelocityScale;

        /// <summary>
        /// Scale factor Unity applies to this object's angular velocity inherited from the Interactor when released.
        /// </summary>
        /// <seealso cref="throwOnDetach"/>
        public float throwAngularVelocityScale
        {
            get => m_ThrowAngularVelocityScale;
            set => m_ThrowAngularVelocityScale = value;
        }

        [SerializeField, FormerlySerializedAs("m_GravityOnDetach")]
        bool m_ForceGravityOnDetach;

        /// <summary>
        /// Forces this object to have gravity when released
        /// (will still use pre-grab value if this is <see langword="false"/>).
        /// </summary>
        public bool forceGravityOnDetach
        {
            get => m_ForceGravityOnDetach;
            set => m_ForceGravityOnDetach = value;
        }

        [SerializeField]
        bool m_RetainTransformParent = true;

        /// <summary>
        /// Whether Unity sets the parent of this object back to its original parent this object was a child of after this object is dropped.
        /// </summary>
        public bool retainTransformParent
        {
            get => m_RetainTransformParent;
            set => m_RetainTransformParent = value;
        }

        [SerializeField]
        AttachPointCompatibilityMode m_AttachPointCompatibilityMode = AttachPointCompatibilityMode.Default;

        /// <summary>
        /// Controls the method used when calculating the target position of the object.
        /// Use <see cref="AttachPointCompatibilityMode.Default"/> for consistent attach points
        /// between all <see cref="XRBaseInteractable.MovementType"/> values.
        /// Marked for deprecation, this property will be removed in a future version.
        /// </summary>
        /// <remarks>
        /// This is a backwards compatibility option in order to keep the old, incorrect method
        /// of calculating the attach point. Projects that already accounted for the difference
        /// can use the Legacy option to maintain the same attach positioning from older versions
        /// without needing to modify the Attach Transform position.
        /// </remarks>
        /// <seealso cref="AttachPointCompatibilityMode"/>
        public AttachPointCompatibilityMode attachPointCompatibilityMode
        {
            get => m_AttachPointCompatibilityMode;
            set => m_AttachPointCompatibilityMode = value;
        }

        [SerializeField]
        List<XRBaseGrabTransformer> m_StartingSingleGrabTransformers = new List<XRBaseGrabTransformer>();

        /// <summary>
        /// The grab transformers that this Interactable automatically links at startup (optional, may be empty).
        /// These are used when there is a single interactor selecting this object.
        /// </summary>
        /// <remarks>
        /// To modify the grab transformers used after startup,
        /// the <see cref="AddSingleGrabTransformer"/> or <see cref="RemoveSingleGrabTransformer"/> methods should be used instead.
        /// </remarks>
        /// <seealso cref="startingMultipleGrabTransformers"/>
        public List<XRBaseGrabTransformer> startingSingleGrabTransformers
        {
            get => m_StartingSingleGrabTransformers;
            set => m_StartingSingleGrabTransformers = value;
        }

        [SerializeField]
        List<XRBaseGrabTransformer> m_StartingMultipleGrabTransformers = new List<XRBaseGrabTransformer>();

        /// <summary>
        /// The grab transformers that this Interactable automatically links at startup (optional, may be empty).
        /// These are used when there are multiple interactors selecting this object.
        /// </summary>
        /// <remarks>
        /// To modify the grab transformers used after startup,
        /// the <see cref="AddMultipleGrabTransformer"/> or <see cref="RemoveMultipleGrabTransformer"/> methods should be used instead.
        /// </remarks>
        /// <seealso cref="startingSingleGrabTransformers"/>
        public List<XRBaseGrabTransformer> startingMultipleGrabTransformers
        {
            get => m_StartingMultipleGrabTransformers;
            set => m_StartingMultipleGrabTransformers = value;
        }

        [SerializeField]
        bool m_AddDefaultGrabTransformers = true;

        /// <summary>
        /// Whether Unity will add the default set of grab transformers if either the Single or Multiple Grab Transformers lists are empty.
        /// </summary>
        /// <remarks>
        /// Set this to <see langword="false"/> if you want to manually set the grab transformers used by populating
        /// <see cref="startingSingleGrabTransformers"/> and <see cref="startingMultipleGrabTransformers"/>.
        /// </remarks>
        /// <seealso cref="AddDefaultSingleGrabTransformer"/>
        /// <seealso cref="AddDefaultMultipleGrabTransformer"/>
        public bool addDefaultGrabTransformers
        {
            get => m_AddDefaultGrabTransformers;
            set => m_AddDefaultGrabTransformers = value;
        }

        /// <summary>
        /// The number of single grab transformers.
        /// These are the grab transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <seealso cref="AddSingleGrabTransformer"/>
        public int singleGrabTransformersCount => m_SingleGrabTransformers.flushedCount;

        /// <summary>
        /// The number of multiple grab transformers.
        /// These are the grab transformers used when there are multiple interactors selecting this object.
        /// </summary>
        /// <seealso cref="AddMultipleGrabTransformer"/>
        public int multipleGrabTransformersCount => m_MultipleGrabTransformers.flushedCount;

        readonly SmallRegistrationList<IXRGrabTransformer> m_SingleGrabTransformers = new SmallRegistrationList<IXRGrabTransformer>();
        readonly SmallRegistrationList<IXRGrabTransformer> m_MultipleGrabTransformers = new SmallRegistrationList<IXRGrabTransformer>();

        List<IXRGrabTransformer> m_GrabTransformersAddedWhenGrabbed;
        bool m_GrabCountChanged;
        (int, int) m_GrabCountBeforeAndAfterChange;
        bool m_IsProcessingGrabTransformers;

        /// <summary>
        /// The number of registered grab transformers that implement <see cref="IXRDropTransformer"/>.
        /// </summary>
        int m_DropTransformersCount;
        static readonly LinkedPool<DropEventArgs> s_DropEventArgs = new LinkedPool<DropEventArgs>(() => new DropEventArgs(), collectionCheck: false);

        // World pose we are moving towards each frame (eventually will be at Interactor's attach point assuming default single grab algorithm)
        Pose m_TargetPose;
        Vector3 m_TargetLocalScale;

        bool m_IsTargetPoseDirty;
        bool m_IsTargetLocalScaleDirty;

        bool isTransformDirty
        {
            get => m_IsTargetPoseDirty || m_IsTargetLocalScaleDirty;
            set
            {
                m_IsTargetPoseDirty = value;
                m_IsTargetLocalScaleDirty = value;
            }
        }

        float m_CurrentAttachEaseTime;
        MovementType m_CurrentMovementType;

        bool m_DetachInLateUpdate;
        Vector3 m_DetachLinearVelocity;
        Vector3 m_DetachAngularVelocity;

        int m_ThrowSmoothingCurrentFrame;
        readonly float[] m_ThrowSmoothingFrameTimes = new float[k_ThrowSmoothingFrameCount];
        readonly Vector3[] m_ThrowSmoothingLinearVelocityFrames = new Vector3[k_ThrowSmoothingFrameCount];
        readonly Vector3[] m_ThrowSmoothingAngularVelocityFrames = new Vector3[k_ThrowSmoothingFrameCount];
        bool m_ThrowSmoothingFirstUpdate;
        Pose m_LastThrowReferencePose;
        IXRAimAssist m_ThrowAssist;

        Rigidbody m_Rigidbody;

        // Rigidbody's settings upon select, kept to restore these values when dropped
        bool m_WasKinematic;
        bool m_UsedGravity;
        float m_OldLinearDamping;
        float m_OldAngularDamping;

        // Used to keep track of colliders for which to ignore collision with character only while grabbed
        bool m_IgnoringCharacterCollision;
        bool m_StopIgnoringCollisionInLateUpdate;
        CharacterController m_SelectingCharacterController;
        readonly HashSet<IXRSelectInteractor> m_SelectingCharacterInteractors = new HashSet<IXRSelectInteractor>();
        readonly List<Collider> m_RigidbodyColliders = new List<Collider>();
        readonly HashSet<Collider> m_CollidersThatAllowedCharacterCollision = new HashSet<Collider>();

        Transform m_OriginalSceneParent;

        // Account for teleportation to avoid throws with unintentionally high energy
        TeleportationMonitor m_TeleportationMonitor;

        readonly Dictionary<IXRSelectInteractor, Transform> m_DynamicAttachTransforms = new Dictionary<IXRSelectInteractor, Transform>();

        static readonly LinkedPool<Transform> s_DynamicAttachTransformPool = new LinkedPool<Transform>(OnCreatePooledItem, OnGetPooledItem, OnReleasePooledItem, OnDestroyPooledItem);

        static readonly ProfilerMarker s_ProcessGrabTransformersMarker = new ProfilerMarker("XRI.ProcessGrabTransformers");

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            m_TeleportationMonitor = new TeleportationMonitor();
            m_TeleportationMonitor.teleported += OnTeleported;

            m_CurrentMovementType = m_MovementType;
            if (!TryGetComponent(out m_Rigidbody))
                Debug.LogError("XR Grab Interactable does not have a required Rigidbody.", this);

            m_Rigidbody.GetComponentsInChildren(true, m_RigidbodyColliders);
            for (var i = m_RigidbodyColliders.Count - 1; i >= 0; i--)
            {
                if (m_RigidbodyColliders[i].attachedRigidbody != m_Rigidbody)
                    m_RigidbodyColliders.RemoveAt(i);
            }

            InitializeTargetPoseAndScale(transform);

            if (m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Legacy)
            {
#pragma warning disable 618 // Adding deprecated component to help with backwards compatibility with existing user projects.
                var legacyGrabTransformer = GetOrAddComponent<XRLegacyGrabTransformer>();
#pragma warning restore 618
                legacyGrabTransformer.enabled = true;
                return;
            }

            // Load the starting grab transformers into the Play mode lists.
            // It is more efficient to add than move, but if there are existing items
            // use move to ensure the correct order dictated by the starting lists.
            if (m_SingleGrabTransformers.flushedCount > 0)
            {
                var index = 0;
                foreach (var transformer in m_StartingSingleGrabTransformers)
                {
                    if (transformer != null)
                        MoveSingleGrabTransformerTo(transformer, index++);
                }
            }
            else
            {
                foreach (var transformer in m_StartingSingleGrabTransformers)
                {
                    if (transformer != null)
                        AddSingleGrabTransformer(transformer);
                }
            }

            if (m_MultipleGrabTransformers.flushedCount > 0)
            {
                var index = 0;
                foreach (var transformer in m_StartingMultipleGrabTransformers)
                {
                    if (transformer != null)
                        MoveMultipleGrabTransformerTo(transformer, index++);
                }
            }
            else
            {
                foreach (var transformer in m_StartingMultipleGrabTransformers)
                {
                    if (transformer != null)
                        AddMultipleGrabTransformer(transformer);
                }
            }

            FlushRegistration();
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            // Unlink this interactable from the grab transformers
            ClearSingleGrabTransformers();
            ClearMultipleGrabTransformers();
            base.OnDestroy();
        }

        /// <inheritdoc />
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            // Add the default grab transformers if needed.
            // This is done here (as opposed to Awake) since transformer behaviors automatically register in their Start,
            // so existing components should have a chance to register before we add the default grab transformers.
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                AddDefaultGrabTransformers();

            FlushRegistration();

            switch (updatePhase)
            {
                // During Fixed update we want to apply any Rigidbody-based updates (e.g., Kinematic or VelocityTracking).
                case XRInteractionUpdateOrder.UpdatePhase.Fixed:
                    if (isSelected || isTransformDirty)
                    {
                        if (m_CurrentMovementType == MovementType.Kinematic ||
                            m_CurrentMovementType == MovementType.VelocityTracking)
                        {
                            // If we only updated the target scale externally, just update that.
                            if (m_IsTargetLocalScaleDirty && !m_IsTargetPoseDirty && !isSelected)
                                ApplyTargetScale();
                            else if (m_CurrentMovementType == MovementType.Kinematic)
                                PerformKinematicUpdate(updatePhase);
                            else if (m_CurrentMovementType == MovementType.VelocityTracking)
                                PerformVelocityTrackingUpdate(updatePhase, Time.deltaTime);
                        }
                    }

                    if (m_IgnoringCharacterCollision && !m_StopIgnoringCollisionInLateUpdate &&
                        m_SelectingCharacterInteractors.Count == 0 && m_SelectingCharacterController != null &&
                        IsOutsideCharacterCollider(m_SelectingCharacterController))
                    {
                        // Wait until Late update so that physics can update before we restore the ability to collide with character
                        m_StopIgnoringCollisionInLateUpdate = true;
                    }
                    break;

                // During Dynamic update and OnBeforeRender we want to update the target pose and apply any Transform-based updates (e.g., Instantaneous).
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                    if (isTransformDirty)
                    {
                        // If we only updated the target scale externally, just update that.
                        if (m_IsTargetLocalScaleDirty && !m_IsTargetPoseDirty)
                            ApplyTargetScale();
                        else
                            PerformInstantaneousUpdate(updatePhase);
                    }

                    if (isSelected || (m_GrabCountChanged && m_DropTransformersCount > 0))
                    {
                        UpdateTarget(updatePhase, Time.deltaTime);

                        if (m_CurrentMovementType == MovementType.Instantaneous)
                            PerformInstantaneousUpdate(updatePhase);
                    }
                    break;

                // Late update is used to handle detach and restoring character collision as late as possible.
                case XRInteractionUpdateOrder.UpdatePhase.Late:
                    if (m_DetachInLateUpdate)
                    {
                        if (!isSelected)
                            Detach();
                        m_DetachInLateUpdate = false;
                    }

                    if (m_StopIgnoringCollisionInLateUpdate)
                    {
                        if (m_IgnoringCharacterCollision && m_SelectingCharacterController != null)
                        {
                            StopIgnoringCharacterCollision(m_SelectingCharacterController);
                            m_SelectingCharacterController = null;
                        }

                        m_StopIgnoringCollisionInLateUpdate = false;
                    }
                    break;
            }
        }

        /// <inheritdoc />
        public override Transform GetAttachTransform(IXRInteractor interactor)
        {
            bool isFirst = interactorsSelecting.Count <= 1 || ReferenceEquals(interactor, interactorsSelecting[0]);

            // If first selector, do normal behavior.
            // If second, we ignore dynamic attach setting if there is no secondary attach transform.
            var shouldUseDynamicAttach = m_UseDynamicAttach || (!isFirst && m_SecondaryAttachTransform == null);

            if (shouldUseDynamicAttach && interactor is IXRSelectInteractor selectInteractor &&
                m_DynamicAttachTransforms.TryGetValue(selectInteractor, out var dynamicAttachTransform))
            {
                if (dynamicAttachTransform != null)
                    return dynamicAttachTransform;

                m_DynamicAttachTransforms.Remove(selectInteractor);
                Debug.LogWarning($"Dynamic Attach Transform created by {this} for {interactor} was destroyed after being created." +
                    " Continuing as if Use Dynamic Attach was disabled for this pair.", this);
            }

            // If not first, and not using dynamic attach, then we must have a secondary attach transform set.
            if (!isFirst && !shouldUseDynamicAttach)
            {
                return m_SecondaryAttachTransform;
            }

            return m_AttachTransform != null ? m_AttachTransform : base.GetAttachTransform(interactor);
        }

        /// <summary>
        /// Adds the given grab transformer to the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <param name="transformer">The grab transformer to add.</param>
        /// <seealso cref="AddMultipleGrabTransformer"/>
        public void AddSingleGrabTransformer(IXRGrabTransformer transformer) => AddGrabTransformer(transformer, m_SingleGrabTransformers);

        /// <summary>
        /// Adds the given grab transformer to the list of transformers used when there are multiple interactors selecting this object.
        /// </summary>
        /// <param name="transformer">The grab transformer to add.</param>
        /// <seealso cref="AddSingleGrabTransformer"/>
        public void AddMultipleGrabTransformer(IXRGrabTransformer transformer) => AddGrabTransformer(transformer, m_MultipleGrabTransformers);

        /// <summary>
        /// Removes the given grab transformer from the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <param name="transformer">The grab transformer to remove.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="transformer"/> was removed from the list.
        /// Otherwise, returns <see langword="false"/> if <paramref name="transformer"/> was not found in the list.
        /// </returns>
        /// <seealso cref="RemoveMultipleGrabTransformer"/>
        public bool RemoveSingleGrabTransformer(IXRGrabTransformer transformer) => RemoveGrabTransformer(transformer, m_SingleGrabTransformers);

        /// <summary>
        /// Removes the given grab transformer from the list of transformers used when there is are multiple interactors selecting this object.
        /// </summary>
        /// <param name="transformer">The grab transformer to remove.</param>
        /// <returns>
        /// Returns <see langword="true"/> if <paramref name="transformer"/> was removed from the list.
        /// Otherwise, returns <see langword="false"/> if <paramref name="transformer"/> was not found in the list.
        /// </returns>
        /// <seealso cref="RemoveSingleGrabTransformer"/>
        public bool RemoveMultipleGrabTransformer(IXRGrabTransformer transformer) => RemoveGrabTransformer(transformer, m_MultipleGrabTransformers);

        /// <summary>
        /// Removes all grab transformers from the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <seealso cref="ClearMultipleGrabTransformers"/>
        public void ClearSingleGrabTransformers() => ClearGrabTransformers(m_SingleGrabTransformers);

        /// <summary>
        /// Removes all grab transformers from the list of transformers used when there is are multiple interactors selecting this object.
        /// </summary>
        /// <seealso cref="ClearSingleGrabTransformers"/>
        public void ClearMultipleGrabTransformers() => ClearGrabTransformers(m_MultipleGrabTransformers);

        /// <summary>
        /// Returns all transformers used when there is a single interactor selecting this object into List <paramref name="results"/>.
        /// </summary>
        /// <param name="results">List to receive grab transformers.</param>
        /// <remarks>
        /// This method populates the list with the grab transformers at the time the
        /// method is called. It is not a live view, meaning grab transformers
        /// added or removed afterward will not be reflected in the
        /// results of this method.
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        /// <seealso cref="GetMultipleGrabTransformers"/>
        public void GetSingleGrabTransformers(List<IXRGrabTransformer> results) => GetGrabTransformers(m_SingleGrabTransformers, results);

        /// <summary>
        /// Returns all transformers used when there are multiple interactors selecting this object into List <paramref name="results"/>.
        /// </summary>
        /// <param name="results">List to receive grab transformers.</param>
        /// <remarks>
        /// This method populates the list with the grab transformers at the time the
        /// method is called. It is not a live view, meaning grab transformers
        /// added or removed afterward will not be reflected in the
        /// results of this method.
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        /// <seealso cref="GetSingleGrabTransformers"/>
        public void GetMultipleGrabTransformers(List<IXRGrabTransformer> results) => GetGrabTransformers(m_MultipleGrabTransformers, results);

        /// <summary>
        /// Returns the grab transformer at <paramref name="index"/> in the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <param name="index">Index of the grab transformer to return. Must be smaller than <see cref="singleGrabTransformersCount"/> and not negative.</param>
        /// <returns>Returns the grab transformer at the given index.</returns>
        /// <seealso cref="GetMultipleGrabTransformerAt"/>
        public IXRGrabTransformer GetSingleGrabTransformerAt(int index) => m_SingleGrabTransformers.GetRegisteredItemAt(index);

        /// <summary>
        /// Returns the grab transformer at <paramref name="index"/> in the list of transformers used when there are multiple interactors selecting this object.
        /// </summary>
        /// <param name="index">Index of the grab transformer to return. Must be smaller than <see cref="multipleGrabTransformersCount"/> and not negative.</param>
        /// <returns>Returns the grab transformer at the given index.</returns>
        /// <seealso cref="GetSingleGrabTransformerAt"/>
        public IXRGrabTransformer GetMultipleGrabTransformerAt(int index) => m_MultipleGrabTransformers.GetRegisteredItemAt(index);

        /// <summary>
        /// Moves the given grab transformer in the list of transformers used when there is a single interactor selecting this object.
        /// If the grab transformer is not in the list, this can be used to insert the grab transformer at the specified index.
        /// </summary>
        /// <param name="transformer">The grab transformer to move or add.</param>
        /// <param name="newIndex">New index of the grab transformer.</param>
        /// <seealso cref="MoveMultipleGrabTransformerTo"/>
        public void MoveSingleGrabTransformerTo(IXRGrabTransformer transformer, int newIndex) => MoveGrabTransformerTo(transformer, newIndex, m_SingleGrabTransformers);

        /// <summary>
        /// Moves the given grab transformer in the list of transformers used when there are multiple interactors selecting this object.
        /// If the grab transformer is not in the list, this can be used to insert the grab transformer at the specified index.
        /// </summary>
        /// <param name="transformer">The grab transformer to move or add.</param>
        /// <param name="newIndex">New index of the grab transformer.</param>
        /// <seealso cref="MoveSingleGrabTransformerTo"/>
        public void MoveMultipleGrabTransformerTo(IXRGrabTransformer transformer, int newIndex) => MoveGrabTransformerTo(transformer, newIndex, m_MultipleGrabTransformers);

        /// <summary>
        /// Retrieves the current world space target pose.
        /// </summary>
        /// <returns>Returns the current world space target pose in the form of a <see cref="Pose"/> struct.</returns>
        /// <seealso cref="SetTargetPose"/>
        /// <seealso cref="GetTargetLocalScale"/>
        public Pose GetTargetPose()
        {
            return m_TargetPose;
        }

        /// <summary>
        /// Sets a new world space target pose.
        /// </summary>
        /// <param name="pose">The new world space target pose, represented as a <see cref="Pose"/> struct.</param>
        /// <remarks>
        /// This bypasses easing and smoothing.
        /// </remarks>
        /// <seealso cref="GetTargetPose"/>
        /// <seealso cref="SetTargetLocalScale"/>
        public void SetTargetPose(Pose pose)
        {
            m_TargetPose = pose;

            // If there are no interactors selecting this object, we need to set the target pose dirty
            // so that the pose is applied in the next phase it is applied.
            m_IsTargetPoseDirty = interactorsSelecting.Count == 0;
        }

        /// <summary>
        /// Retrieves the current target local scale.
        /// </summary>
        /// <returns>Returns the current target local scale in the form of a <see cref="Vector3"/> struct.</returns>
        /// <seealso cref="SetTargetLocalScale"/>
        /// <seealso cref="GetTargetPose"/>
        public Vector3 GetTargetLocalScale()
        {
            return m_TargetLocalScale;
        }

        /// <summary>
        /// Sets a new target local scale.
        /// </summary>
        /// <param name="localScale">The new target local scale, represented as a <see cref="Vector3"/> struct.</param>
        /// <remarks>
        /// This bypasses easing and smoothing.
        /// </remarks>
        /// <seealso cref="GetTargetLocalScale"/>
        /// <seealso cref="SetTargetPose"/>
        public void SetTargetLocalScale(Vector3 localScale)
        {
            m_TargetLocalScale = localScale;

            // If there are no interactors selecting this object, we need to set the target local scale dirty
            // so that the pose is applied in the next phase it is applied.
            m_IsTargetLocalScaleDirty = interactorsSelecting.Count == 0;
        }

        void InitializeTargetPoseAndScale(Transform thisTransform)
        {
            m_TargetPose.position = m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default ? thisTransform.position : m_Rigidbody.worldCenterOfMass;
            m_TargetPose.rotation = thisTransform.rotation;
            m_TargetLocalScale = thisTransform.localScale;
        }

        void AddGrabTransformer(IXRGrabTransformer transformer, BaseRegistrationList<IXRGrabTransformer> grabTransformers)
        {
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));

            if (m_IsProcessingGrabTransformers)
                Debug.LogWarning($"{transformer} added while {name} is processing grab transformers. It won't be processed until the next process.", this);

            if (grabTransformers.Register(transformer))
                OnAddedGrabTransformer(transformer);
        }

        bool RemoveGrabTransformer(IXRGrabTransformer transformer, BaseRegistrationList<IXRGrabTransformer> grabTransformers)
        {
            if (grabTransformers.Unregister(transformer))
            {
                OnRemovedGrabTransformer(transformer);
                return true;
            }

            return false;
        }

        void ClearGrabTransformers(BaseRegistrationList<IXRGrabTransformer> grabTransformers)
        {
            for (var index = grabTransformers.flushedCount - 1; index >= 0; --index)
            {
                var transformer = grabTransformers.GetRegisteredItemAt(index);
                RemoveGrabTransformer(transformer, grabTransformers);
            }
        }

        static void GetGrabTransformers(BaseRegistrationList<IXRGrabTransformer> grabTransformers, List<IXRGrabTransformer> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            grabTransformers.GetRegisteredItems(results);
        }

        void MoveGrabTransformerTo(IXRGrabTransformer transformer, int newIndex, BaseRegistrationList<IXRGrabTransformer> grabTransformers)
        {
            if (transformer == null)
                throw new ArgumentNullException(nameof(transformer));

            // BaseRegistrationList<T> does not yet support reordering with pending registration changes.
            if (m_IsProcessingGrabTransformers)
            {
                Debug.LogError($"Cannot move {transformer} while {name} is processing grab transformers.", this);
                return;
            }

            grabTransformers.Flush();
            if (grabTransformers.MoveItemImmediately(transformer, newIndex))
                OnAddedGrabTransformer(transformer);
        }

        void FlushRegistration()
        {
            m_SingleGrabTransformers.Flush();
            m_MultipleGrabTransformers.Flush();
        }

        void InvokeGrabTransformersOnGrab()
        {
            m_IsProcessingGrabTransformers = true;

            if (m_SingleGrabTransformers.registeredSnapshot.Count > 0)
            {
                foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                {
                    if (m_SingleGrabTransformers.IsStillRegistered(transformer))
                        transformer.OnGrab(this);
                }
            }

            if (m_MultipleGrabTransformers.registeredSnapshot.Count > 0)
            {
                foreach (var transformer in m_MultipleGrabTransformers.registeredSnapshot)
                {
                    if (m_MultipleGrabTransformers.IsStillRegistered(transformer))
                        transformer.OnGrab(this);
                }
            }

            m_IsProcessingGrabTransformers = false;
        }

        void InvokeGrabTransformersOnDrop(DropEventArgs args)
        {
            m_IsProcessingGrabTransformers = true;

            if (m_SingleGrabTransformers.registeredSnapshot.Count > 0)
            {
                foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                {
                    if (transformer is IXRDropTransformer dropTransformer && m_SingleGrabTransformers.IsStillRegistered(transformer))
                        dropTransformer.OnDrop(this, args);
                }
            }

            if (m_MultipleGrabTransformers.registeredSnapshot.Count > 0)
            {
                foreach (var transformer in m_MultipleGrabTransformers.registeredSnapshot)
                {
                    if (transformer is IXRDropTransformer dropTransformer && m_MultipleGrabTransformers.IsStillRegistered(transformer))
                        dropTransformer.OnDrop(this, args);
                }
            }

            m_IsProcessingGrabTransformers = false;
        }

        void InvokeGrabTransformersProcess(XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            m_IsProcessingGrabTransformers = true;

            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable -- ProfilerMarker.Begin with context object does not have Pure attribute
            using (s_ProcessGrabTransformersMarker.Auto())
            {
                // Cache some frequently evaluated properties to local variables.
                // The registration lists are not flushed during this method, so these are invariant.
                var grabbed = isSelected;
                var hasSingleGrabTransformer = m_SingleGrabTransformers.registeredSnapshot.Count > 0;
                var hasMultipleGrabTransformer = m_MultipleGrabTransformers.registeredSnapshot.Count > 0;

                // Let the transformers setup if the grab count changed.
                if (m_GrabCountChanged)
                {
                    if (grabbed)
                    {
                        if (hasSingleGrabTransformer)
                        {
                            foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                            {
                                if (m_SingleGrabTransformers.IsStillRegistered(transformer))
                                    transformer.OnGrabCountChanged(this, targetPose, localScale);
                            }
                        }

                        if (hasMultipleGrabTransformer)
                        {
                            foreach (var transformer in m_MultipleGrabTransformers.registeredSnapshot)
                            {
                                if (m_MultipleGrabTransformers.IsStillRegistered(transformer))
                                    transformer.OnGrabCountChanged(this, targetPose, localScale);
                            }
                        }
                    }

                    m_GrabCountChanged = false;
                    m_GrabTransformersAddedWhenGrabbed?.Clear();
                }
                else if (m_GrabTransformersAddedWhenGrabbed?.Count > 0)
                {
                    if (grabbed)
                    {
                        // Calling OnGrabCountChanged on just the grab transformers added when this was already grabbed
                        // avoids calling it needlessly on all linked grab transformers.
                        foreach (var transformer in m_GrabTransformersAddedWhenGrabbed)
                        {
                            transformer.OnGrabCountChanged(this, targetPose, localScale);
                        }
                    }

                    m_GrabTransformersAddedWhenGrabbed.Clear();
                }

                if (grabbed)
                {
                    // Give the Multiple Grab Transformers first chance to process,
                    // and if one actually does, skip the Single Grab Transformers.
                    // Also let the Multiple Grab Transformers process if there aren't any Single Grab Transformers.
                    // An empty Single Grab Transformers list is treated the same as a populated list where none can process.
                    var processed = false;
                    if (hasMultipleGrabTransformer && (interactorsSelecting.Count > 1 || !CanProcessAnySingleGrabTransformer()))
                    {
                        foreach (var transformer in m_MultipleGrabTransformers.registeredSnapshot)
                        {
                            if (!m_MultipleGrabTransformers.IsStillRegistered(transformer))
                                continue;

                            if (transformer.canProcess)
                            {
                                transformer.Process(this, updatePhase, ref targetPose, ref localScale);
                                processed = true;
                            }
                        }
                    }

                    if (!processed && hasSingleGrabTransformer)
                    {
                        foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                        {
                            if (!m_SingleGrabTransformers.IsStillRegistered(transformer))
                                continue;

                            if (transformer.canProcess)
                                transformer.Process(this, updatePhase, ref targetPose, ref localScale);
                        }
                    }
                }
                else
                {
                    // When not selected, we process both Multiple and Single transformers that implement IXRDropTransformer
                    // and do not try to recreate the logic of prioritizing Multiple over Single. The rules for prioritizing
                    // would not be intuitive, so we just process all transformers.
                    if (hasMultipleGrabTransformer)
                    {
                        foreach (var transformer in m_MultipleGrabTransformers.registeredSnapshot)
                        {
                            if (!(transformer is IXRDropTransformer dropTransformer) ||
                                !m_MultipleGrabTransformers.IsStillRegistered(transformer))
                            {
                                continue;
                            }

                            if (dropTransformer.canProcessOnDrop && transformer.canProcess)
                                transformer.Process(this, updatePhase, ref targetPose, ref localScale);
                        }
                    }

                    if (hasSingleGrabTransformer)
                    {
                        foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                        {
                            if (!(transformer is IXRDropTransformer dropTransformer) ||
                                !m_SingleGrabTransformers.IsStillRegistered(transformer))
                            {
                                continue;
                            }

                            if (dropTransformer.canProcessOnDrop && transformer.canProcess)
                                transformer.Process(this, updatePhase, ref targetPose, ref localScale);
                        }
                    }
                }
            }

            m_IsProcessingGrabTransformers = false;
        }

        /// <summary>
        /// Same check as Linq code for: <c>Any(t => t.canProcess)</c>.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the source list is not empty and at least
        /// one element passes the test; otherwise, <see langword="false"/>.</returns>
        bool CanProcessAnySingleGrabTransformer()
        {
            if (m_SingleGrabTransformers.registeredSnapshot.Count > 0)
            {
                foreach (var transformer in m_SingleGrabTransformers.registeredSnapshot)
                {
                    if (!m_SingleGrabTransformers.IsStillRegistered(transformer))
                        continue;

                    if (transformer.canProcess)
                        return true;
                }
            }

            return false;
        }

        void OnAddedGrabTransformer(IXRGrabTransformer transformer)
        {
            if (transformer is IXRDropTransformer)
                ++m_DropTransformersCount;

            transformer.OnLink(this);

            if (interactorsSelecting.Count == 0)
                return;

            // OnGrab is invoked immediately, but OnGrabCountChanged is only invoked right before Process so
            // it must be added to a list to maintain those that still need to have it invoked. It functions
            // like a setup method and users should be able to rely on it always being called at least once
            // when grabbed.
            transformer.OnGrab(this);

            if (m_GrabTransformersAddedWhenGrabbed == null)
                m_GrabTransformersAddedWhenGrabbed = new List<IXRGrabTransformer>();

            m_GrabTransformersAddedWhenGrabbed.Add(transformer);
        }

        void OnRemovedGrabTransformer(IXRGrabTransformer transformer)
        {
            if (transformer is IXRDropTransformer)
                --m_DropTransformersCount;

            transformer.OnUnlink(this);
            m_GrabTransformersAddedWhenGrabbed?.Remove(transformer);
        }

        void AddDefaultGrabTransformers()
        {
            if (!m_AddDefaultGrabTransformers)
                return;

            if (m_SingleGrabTransformers.flushedCount == 0)
                AddDefaultSingleGrabTransformer();

            // Avoid adding the multiple grab transformer component unnecessarily since it may never be needed.
            if (m_MultipleGrabTransformers.flushedCount == 0 && selectMode == InteractableSelectMode.Multiple && interactorsSelecting.Count > 1)
                AddDefaultMultipleGrabTransformer();
        }

        /// <summary>
        /// Adds the default <seealso cref="XRGeneralGrabTransformer"/> (if the Single or Multiple Grab Transformers lists are empty)
        /// to the list of transformers used when there is a single interactor selecting this object.
        /// </summary>
        /// <seealso cref="addDefaultGrabTransformers"/>
        protected virtual void AddDefaultSingleGrabTransformer()
        {
            if (m_SingleGrabTransformers.flushedCount == 0)
            {
                var transformer = GetOrAddDefaultGrabTransformer();
                AddSingleGrabTransformer(transformer);
            }
        }

        /// <summary>
        /// Adds the default grab transformer (if the Multiple Grab Transformers list is empty)
        /// to the list of transformers used when there are multiple interactors selecting this object.
        /// </summary>
        /// <seealso cref="addDefaultGrabTransformers"/>
        protected virtual void AddDefaultMultipleGrabTransformer()
        {
            if (m_MultipleGrabTransformers.flushedCount == 0)
            {
                var transformer = GetOrAddDefaultGrabTransformer();
                AddMultipleGrabTransformer(transformer);
            }
        }

        IXRGrabTransformer GetOrAddDefaultGrabTransformer()
        {
            return GetOrAddComponent<XRGeneralGrabTransformer>();
        }

        T GetOrAddComponent<T>() where T : Component
        {
            return TryGetComponent<T>(out var component) ? component : gameObject.AddComponent<T>();
        }

        void UpdateTarget(XRInteractionUpdateOrder.UpdatePhase updatePhase, float deltaTime)
        {
            // If the grab count changed to a lower number, and it is now 1, we need to recompute the dynamic attach transform for the interactor.
            if (m_ReinitializeDynamicAttachEverySingleGrab && m_GrabCountChanged && m_GrabCountBeforeAndAfterChange.Item2 < m_GrabCountBeforeAndAfterChange.Item1 && interactorsSelecting.Count == 1 &&
                m_DynamicAttachTransforms.Count > 0 && m_DynamicAttachTransforms.TryGetValue(interactorsSelecting[0], out var dynamicAttachTransform))
            {
                InitializeDynamicAttachPoseInternal(interactorsSelecting[0], dynamicAttachTransform);
            }

            var rawTargetPose = m_TargetPose;
            var rawTargetScale = m_TargetLocalScale;

            InvokeGrabTransformersProcess(updatePhase, ref rawTargetPose, ref rawTargetScale);

            if (!isSelected)
            {
                m_TargetPose = rawTargetPose;
                m_TargetLocalScale = rawTargetScale;
                return;
            }

            // Skip during OnBeforeRender since it doesn't require that high accuracy.
            // Skip when not selected since the the detach velocity has already been applied and we no longer need to compute it.
            // This means that the final Process for drop grab transformers does not contribute to throw velocity.
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // Track the target pose before easing.
                // This avoids an unintentionally high detach velocity if grabbing with an XRRayInteractor
                // with Force Grab enabled causing the target pose to move very quickly between this transform's
                // initial position and the target pose after easing when the easing time is short.
                // By always tracking the target pose result from the grab transformers, it avoids the issue.
                StepThrowSmoothing(rawTargetPose, deltaTime);
            }

            // Apply easing and smoothing (if configured)
            StepSmoothing(rawTargetPose, rawTargetScale, deltaTime);
        }

        void StepSmoothing(in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime)
        {
            if (m_AttachEaseInTime > 0f && m_CurrentAttachEaseTime <= m_AttachEaseInTime)
            {
                EaseAttachBurst(ref m_TargetPose, ref m_TargetLocalScale, rawTargetPose, rawTargetLocalScale, deltaTime,
                    m_AttachEaseInTime, ref m_CurrentAttachEaseTime);
            }
            else
            {
                StepSmoothingBurst(ref m_TargetPose, ref m_TargetLocalScale, rawTargetPose, rawTargetLocalScale, deltaTime,
                    m_SmoothPosition, m_SmoothPositionAmount, m_TightenPosition,
                    m_SmoothRotation, m_SmoothRotationAmount, m_TightenRotation,
                    m_SmoothScale, m_SmoothScaleAmount, m_TightenScale);
            }
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void EaseAttachBurst(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime,
            float attachEaseInTime, ref float currentAttachEaseTime)
        {
            var easePercent = currentAttachEaseTime / attachEaseInTime;
            targetPose.position = math.lerp(targetPose.position, rawTargetPose.position, easePercent);
            targetPose.rotation = math.slerp(targetPose.rotation, rawTargetPose.rotation, easePercent);
            targetLocalScale =  math.lerp(targetLocalScale, rawTargetLocalScale, easePercent);
            currentAttachEaseTime += deltaTime;
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void StepSmoothingBurst(ref Pose targetPose, ref Vector3 targetLocalScale, in Pose rawTargetPose, in Vector3 rawTargetLocalScale, float deltaTime,
            bool smoothPos, float smoothPosAmount, float tightenPos,
            bool smoothRot, float smoothRotAmount, float tightenRot,
            bool smoothScale, float smoothScaleAmount, float tightenScale)
        {
            if (smoothPos)
            {
                targetPose.position = math.lerp(targetPose.position, rawTargetPose.position, smoothPosAmount * deltaTime);
                targetPose.position = math.lerp(targetPose.position, rawTargetPose.position, tightenPos);
            }
            else
            {
                targetPose.position = rawTargetPose.position;
            }

            if (smoothRot)
            {
                targetPose.rotation = math.slerp(targetPose.rotation, rawTargetPose.rotation, smoothRotAmount * deltaTime);
                targetPose.rotation = math.slerp(targetPose.rotation, rawTargetPose.rotation, tightenRot);
            }
            else
            {
                targetPose.rotation = rawTargetPose.rotation;
            }

            if (smoothScale)
            {
                targetLocalScale = math.lerp(targetLocalScale, rawTargetLocalScale, smoothScaleAmount * deltaTime);
                targetLocalScale = math.lerp(targetLocalScale, rawTargetLocalScale, tightenScale);
            }
            else
            {
                targetLocalScale = rawTargetLocalScale;
            }
        }

        void PerformInstantaneousUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic ||
                updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
            {
                if (m_TrackPosition && m_TrackRotation)
                    transform.SetPositionAndRotation(m_TargetPose.position, m_TargetPose.rotation);
                else if (m_TrackPosition)
                    transform.position = m_TargetPose.position;
                else if (m_TrackRotation)
                    transform.rotation = m_TargetPose.rotation;

                ApplyTargetScale();

                isTransformDirty = false;
            }
        }

        void PerformKinematicUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                if (m_TrackPosition)
                {
                    var position = m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default
                        ? m_TargetPose.position
                        : m_TargetPose.position - m_Rigidbody.worldCenterOfMass + m_Rigidbody.position;
                    m_Rigidbody.MovePosition(position);
                }

                if (m_TrackRotation)
                    m_Rigidbody.MoveRotation(m_TargetPose.rotation);

                ApplyTargetScale();

                isTransformDirty = false;
            }
        }

        void PerformVelocityTrackingUpdate(XRInteractionUpdateOrder.UpdatePhase updatePhase, float deltaTime)
        {
            // Skip velocity calculations if Time.deltaTime is too low due to a frame-timing issue on Quest
            if (deltaTime < k_DeltaTimeThreshold)
                return;

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                // Do linear velocity tracking
                if (m_TrackPosition)
                {
                    // Scale initialized velocity by prediction factor
#if UNITY_2023_3_OR_NEWER
                    m_Rigidbody.linearVelocity *= (1f - m_VelocityDamping);
#else
                    m_Rigidbody.velocity *= (1f - m_VelocityDamping);
#endif
                    var positionDelta = m_AttachPointCompatibilityMode == AttachPointCompatibilityMode.Default
                        ? m_TargetPose.position - transform.position
                        : m_TargetPose.position - m_Rigidbody.worldCenterOfMass;
                    var velocity = positionDelta / deltaTime;
#if UNITY_2023_3_OR_NEWER
                    m_Rigidbody.linearVelocity += (velocity * m_VelocityScale);
#else
                    m_Rigidbody.velocity += (velocity * m_VelocityScale);
#endif
                }

                // Do angular velocity tracking
                if (m_TrackRotation)
                {
                    // Scale initialized velocity by prediction factor
                    m_Rigidbody.angularVelocity *= (1f - m_AngularVelocityDamping);
                    var rotationDelta = m_TargetPose.rotation * Quaternion.Inverse(transform.rotation);
                    rotationDelta.ToAngleAxis(out var angleInDegrees, out var rotationAxis);
                    if (angleInDegrees > 180f)
                        angleInDegrees -= 360f;

                    if (Mathf.Abs(angleInDegrees) > Mathf.Epsilon)
                    {
                        var angularVelocity = (rotationAxis * (angleInDegrees * Mathf.Deg2Rad)) / deltaTime;
                        m_Rigidbody.angularVelocity += (angularVelocity * m_AngularVelocityScale);
                    }
                }

                ApplyTargetScale();

                isTransformDirty = false;
            }
        }

        void ApplyTargetScale()
        {
            if (m_TrackScale)
                transform.localScale = m_TargetLocalScale;

            m_IsTargetLocalScaleDirty = false;
        }

        void UpdateCurrentMovementType()
        {
            // Special case where the interactor will override this objects movement type (used for Sockets and other absolute interactors).
            // Iterates in reverse order so the most recent interactor with an override will win since that seems like it would
            // be the strategy most users would want by default.
            MovementType? movementTypeOverride = null;
            for (var index = interactorsSelecting.Count - 1; index >= 0; --index)
            {
                var baseInteractor = interactorsSelecting[index] as XRBaseInteractor;
                if (baseInteractor != null && baseInteractor.selectedInteractableMovementTypeOverride.HasValue)
                {
                    if (movementTypeOverride.HasValue)
                    {
                        Debug.LogWarning($"Multiple interactors selecting \"{name}\" have different movement type override values set" +
                            $" ({nameof(XRBaseInteractor.selectedInteractableMovementTypeOverride)})." +
                            $" Conflict resolved using {movementTypeOverride.Value} from the most recent interactor to select this object with an override.", this);
                        break;
                    }

                    movementTypeOverride = baseInteractor.selectedInteractableMovementTypeOverride.Value;
                }
            }

            m_CurrentMovementType = movementTypeOverride ?? m_MovementType;
        }

        /// <inheritdoc />
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            // Setup the dynamic attach transform.
            // Done before calling the base method so the attach pose captured is the dynamic one.
            var dynamicAttachTransform = CreateDynamicAttachTransform(args.interactorObject);
            InitializeDynamicAttachPoseInternal(args.interactorObject, dynamicAttachTransform);

            // Store the grab count change.
            var grabCountBeforeChange = interactorsSelecting.Count;
            base.OnSelectEntering(args);
            var grabCountAfterChange = interactorsSelecting.Count;

            m_GrabCountChanged = true;
            m_GrabCountBeforeAndAfterChange = (grabCountBeforeChange, grabCountAfterChange);
            m_CurrentAttachEaseTime = 0f;

            // Reset the throw data every time the number of grabs increases since
            // each additional grab could cause a large change in target position,
            // making it throw at an unwanted velocity. It is not called when the number
            // of grabs decreases even though it would have the same issue, but doing so
            // would make it almost impossible to throw with both hands.
            ResetThrowSmoothing();

            // Check if we should ignore collision with character every time number of grabs increases since
            // the first select could have happened from a non-character interactor.
            if (!m_IgnoringCharacterCollision)
            {
                m_SelectingCharacterController = args.interactorObject.transform.GetComponentInParent<CharacterController>();
                if (m_SelectingCharacterController != null)
                {
                    m_SelectingCharacterInteractors.Add(args.interactorObject);
                    StartIgnoringCharacterCollision(m_SelectingCharacterController);
                }
            }
            else if (m_SelectingCharacterController != null && args.interactorObject.transform.IsChildOf(m_SelectingCharacterController.transform))
            {
                m_SelectingCharacterInteractors.Add(args.interactorObject);
            }

            if (interactorsSelecting.Count == 1)
            {
                Grab();
                InvokeGrabTransformersOnGrab();
            }

            SubscribeTeleportationProvider(args.interactorObject);
        }

        /// <inheritdoc />
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            // Store the grab count change.
            var grabCountBeforeChange = interactorsSelecting.Count;
            base.OnSelectExiting(args);
            var grabCountAfterChange = interactorsSelecting.Count;

            m_GrabCountChanged = true;
            m_GrabCountBeforeAndAfterChange = (grabCountBeforeChange, grabCountAfterChange);
            m_CurrentAttachEaseTime = 0f;

            if (interactorsSelecting.Count == 0)
            {
                if (m_ThrowOnDetach)
                    m_ThrowAssist = args.interactorObject.transform.GetComponentInParent<IXRAimAssist>();

                Drop();

                if (m_DropTransformersCount > 0)
                {
                    using (s_DropEventArgs.Get(out var dropArgs))
                    {
                        dropArgs.selectExitEventArgs = args;
                        InvokeGrabTransformersOnDrop(dropArgs);
                    }
                }
            }

            // Don't restore ability to collide with character until the object is not overlapping with the character.
            // This prevents the character from being pushed out of the way of the dropped object while moving.
            m_SelectingCharacterInteractors.Remove(args.interactorObject);

            UnsubscribeTeleportationProvider(args.interactorObject);
        }

        /// <inheritdoc />
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            ReleaseDynamicAttachTransform(args.interactorObject);
        }

        Transform CreateDynamicAttachTransform(IXRSelectInteractor interactor)
        {
            Transform dynamicAttachTransform;

            do
            {
                dynamicAttachTransform = s_DynamicAttachTransformPool.Get();
            } while (dynamicAttachTransform == null);

#if UNITY_EDITOR
            dynamicAttachTransform.name = $"[{interactor.transform.name}] Dynamic Attach";
#endif
            dynamicAttachTransform.SetParent(transform, false);

            return dynamicAttachTransform;
        }

        void InitializeDynamicAttachPoseInternal(IXRSelectInteractor interactor, Transform dynamicAttachTransform)
        {
            // InitializeDynamicAttachPose expects it to be initialized with the static pose first
            InitializeDynamicAttachPoseWithStatic(interactor, dynamicAttachTransform);
            InitializeDynamicAttachPose(interactor, dynamicAttachTransform);
        }

        void InitializeDynamicAttachPoseWithStatic(IXRSelectInteractor interactor, Transform dynamicAttachTransform)
        {
            m_DynamicAttachTransforms.Remove(interactor);
            var staticAttachTransform = GetAttachTransform(interactor);
            m_DynamicAttachTransforms[interactor] = dynamicAttachTransform;

            // Base the initial pose on the Attach Transform.
            // Technically we could just do the final else statement, but setting the local position and rotation this way
            // keeps the position and rotation seen in the Inspector tidier by exactly matching instead of potentially having small
            // floating point offsets.
            if (staticAttachTransform == transform)
            {
                dynamicAttachTransform.localPosition = Vector3.zero;
                dynamicAttachTransform.localRotation = Quaternion.identity;
            }
            else if (staticAttachTransform.parent == transform)
            {
                dynamicAttachTransform.localPosition = staticAttachTransform.localPosition;
                dynamicAttachTransform.localRotation = staticAttachTransform.localRotation;
            }
            else
            {
                dynamicAttachTransform.SetPositionAndRotation(staticAttachTransform.position, staticAttachTransform.rotation);
            }
        }

        void ReleaseDynamicAttachTransform(IXRSelectInteractor interactor)
        {
            // Skip checking m_UseDynamicAttach since it may have changed after being grabbed,
            // and we should ensure it is released. We instead check Count first as a faster way to avoid hashing
            // and the Dictionary lookup, which should handle when it was never enabled in the first place.
            if (m_DynamicAttachTransforms.Count > 0 && m_DynamicAttachTransforms.TryGetValue(interactor, out var dynamicAttachTransform))
            {
                if (dynamicAttachTransform != null)
                    s_DynamicAttachTransformPool.Release(dynamicAttachTransform);

                m_DynamicAttachTransforms.Remove(interactor);
            }
        }

        /// <summary>
        /// Unity calls this method automatically when initializing the dynamic attach pose.
        /// Used to override <see cref="matchAttachPosition"/> for a specific interactor.
        /// </summary>
        /// <param name="interactor">The interactor that is initiating the selection.</param>
        /// <returns>Returns whether to match the position of the interactor's attachment point when initializing the grab.</returns>
        /// <seealso cref="matchAttachPosition"/>
        /// <seealso cref="InitializeDynamicAttachPose"/>
        protected virtual bool ShouldMatchAttachPosition(IXRSelectInteractor interactor)
        {
            if (!m_MatchAttachPosition)
                return false;

            // We assume the static pose should always be used for sockets.
            // For Ray Interactors that bring the object to hand (Force Grab enabled), we assume that property
            // takes precedence since otherwise this interactable wouldn't move if we copied the interactor's attach position,
            // which would violate the interactor's expected behavior.
            if (interactor is XRSocketInteractor ||
                interactor is XRRayInteractor rayInteractor && rayInteractor.useForceGrab)
                return false;

            return true;
        }

        /// <summary>
        /// Unity calls this method automatically when initializing the dynamic attach pose.
        /// Used to override <see cref="matchAttachRotation"/> for a specific interactor.
        /// </summary>
        /// <param name="interactor">The interactor that is initiating the selection.</param>
        /// <returns>Returns whether to match the rotation of the interactor's attachment point when initializing the grab.</returns>
        /// <seealso cref="matchAttachRotation"/>
        /// <seealso cref="InitializeDynamicAttachPose"/>
        protected virtual bool ShouldMatchAttachRotation(IXRSelectInteractor interactor)
        {
            // We assume the static pose should always be used for sockets.
            // Unlike for position, we allow a Ray Interactor with Force Grab enabled to match the rotation
            // based on the property in this behavior.
            return m_MatchAttachRotation && !(interactor is XRSocketInteractor);
        }

        /// <summary>
        /// Unity calls this method automatically when initializing the dynamic attach pose.
        /// Used to override <see cref="snapToColliderVolume"/> for a specific interactor.
        /// </summary>
        /// <param name="interactor">The interactor that is initiating the selection.</param>
        /// <returns>Returns whether to adjust the dynamic attachment point to keep it on or inside the Colliders that make up this object.</returns>
        /// <seealso cref="snapToColliderVolume"/>
        /// <seealso cref="InitializeDynamicAttachPose"/>
        protected virtual bool ShouldSnapToColliderVolume(IXRSelectInteractor interactor)
        {
            return m_SnapToColliderVolume;
        }

        /// <summary>
        /// Unity calls this method automatically when the interactor first initiates selection of this interactable.
        /// Override this method to set the pose of the dynamic attachment point. Before this method is called, the transform
        /// is already set as a child GameObject with inherited Transform values.
        /// </summary>
        /// <param name="interactor">The interactor that is initiating the selection.</param>
        /// <param name="dynamicAttachTransform">The dynamic attachment Transform that serves as the attachment point for the given interactor.</param>
        /// <remarks>
        /// This method is only called when <see cref="useDynamicAttach"/> is enabled.
        /// </remarks>
        /// <seealso cref="useDynamicAttach"/>
        protected virtual void InitializeDynamicAttachPose(IXRSelectInteractor interactor, Transform dynamicAttachTransform)
        {
            var matchPosition = ShouldMatchAttachPosition(interactor);
            var matchRotation = ShouldMatchAttachRotation(interactor);
            if (!matchPosition && !matchRotation)
                return;

            // Copy the pose of the interactor's attach transform
            var interactorAttachTransform = interactor.GetAttachTransform(this);
            var position = interactorAttachTransform.position;
            var rotation = interactorAttachTransform.rotation;

            // Optionally constrain the position to within the Collider(s) of this Interactable
            if (matchPosition && ShouldSnapToColliderVolume(interactor) &&
                XRInteractableUtility.TryGetClosestPointOnCollider(this, position, out var distanceInfo))
            {
                position = distanceInfo.point;
            }

            if (matchPosition && matchRotation)
                dynamicAttachTransform.SetPositionAndRotation(position, rotation);
            else if (matchPosition)
                dynamicAttachTransform.position = position;
            else
                dynamicAttachTransform.rotation = rotation;
        }

        /// <summary>
        /// Updates the state of the object due to being grabbed.
        /// Automatically called when entering the Select state.
        /// </summary>
        /// <seealso cref="Drop"/>
        protected virtual void Grab()
        {
            var thisTransform = transform;
            m_OriginalSceneParent = thisTransform.parent;
            thisTransform.SetParent(null);

            UpdateCurrentMovementType();
            SetupRigidbodyGrab(m_Rigidbody);

            // Reset detach velocities
            m_DetachLinearVelocity = Vector3.zero;
            m_DetachAngularVelocity = Vector3.zero;

            // Initialize target pose and scale
            InitializeTargetPoseAndScale(thisTransform);
        }

        /// <summary>
        /// Updates the state of the object due to being dropped and schedule to finish the detach during the end of the frame.
        /// Automatically called when exiting the Select state.
        /// </summary>
        /// <seealso cref="Detach"/>
        /// <seealso cref="Grab"/>
        protected virtual void Drop()
        {
            if (m_RetainTransformParent && m_OriginalSceneParent != null && !m_OriginalSceneParent.gameObject.activeInHierarchy)
            {
#if UNITY_EDITOR
                // Suppress the warning when exiting Play mode to avoid confusing the user
                var exitingPlayMode = UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
                var exitingPlayMode = false;
#endif
                if (!exitingPlayMode)
                    Debug.LogWarning("Retain Transform Parent is set to true, and has a non-null Original Scene Parent. " +
                        "However, the old parent is deactivated so we are choosing not to re-parent upon dropping.", this);
            }
            else if (m_RetainTransformParent && gameObject.activeInHierarchy)
                transform.SetParent(m_OriginalSceneParent);

            SetupRigidbodyDrop(m_Rigidbody);

            m_CurrentMovementType = m_MovementType;
            m_DetachInLateUpdate = true;
            EndThrowSmoothing();
        }

        /// <summary>
        /// Updates the state of the object to finish the detach after being dropped.
        /// Automatically called during the end of the frame after being dropped.
        /// </summary>
        /// <remarks>
        /// This method updates the velocity of the Rigidbody if configured to do so.
        /// </remarks>
        /// <seealso cref="Drop"/>
        protected virtual void Detach()
        {
            if (m_ThrowOnDetach)
            {
                if (m_Rigidbody.isKinematic)
                {
                    Debug.LogWarning("Cannot throw a kinematic Rigidbody since updating the velocity and angular velocity of a kinematic Rigidbody is not supported. Disable Throw On Detach or Is Kinematic to fix this issue.", this);
                    return;
                }

                if (m_ThrowAssist != null)
                {
                    m_DetachLinearVelocity = m_ThrowAssist.GetAssistedVelocity(m_Rigidbody.position, m_DetachLinearVelocity, m_Rigidbody.useGravity ? -Physics.gravity.y : 0f);
                    m_ThrowAssist = null;
                }

#if UNITY_2023_3_OR_NEWER
                m_Rigidbody.linearVelocity = m_DetachLinearVelocity;
#else
                m_Rigidbody.velocity = m_DetachLinearVelocity;
#endif
                m_Rigidbody.angularVelocity = m_DetachAngularVelocity;
            }
        }

        /// <summary>
        /// Setup the <see cref="Rigidbody"/> on this object due to being grabbed.
        /// Automatically called when entering the Select state.
        /// </summary>
        /// <param name="rigidbody">The <see cref="Rigidbody"/> on this object.</param>
        /// <seealso cref="SetupRigidbodyDrop"/>
        // ReSharper disable once ParameterHidesMember
        protected virtual void SetupRigidbodyGrab(Rigidbody rigidbody)
        {
            // Remember Rigidbody settings and setup to move
            m_WasKinematic = rigidbody.isKinematic;
            m_UsedGravity = rigidbody.useGravity;
#if UNITY_2023_3_OR_NEWER
            m_OldLinearDamping = rigidbody.linearDamping;
            m_OldAngularDamping = rigidbody.angularDamping;
#else
            m_OldLinearDamping = rigidbody.drag;
            m_OldAngularDamping = rigidbody.angularDrag;
#endif
            rigidbody.isKinematic = m_CurrentMovementType == MovementType.Kinematic || m_CurrentMovementType == MovementType.Instantaneous;
            rigidbody.useGravity = false;
#if UNITY_2023_3_OR_NEWER
            rigidbody.linearDamping = 0f;
            rigidbody.angularDamping = 0f;
#else
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
#endif
        }

        /// <summary>
        /// Setup the <see cref="Rigidbody"/> on this object due to being dropped.
        /// Automatically called when exiting the Select state.
        /// </summary>
        /// <param name="rigidbody">The <see cref="Rigidbody"/> on this object.</param>
        /// <seealso cref="SetupRigidbodyGrab"/>
        // ReSharper disable once ParameterHidesMember
        protected virtual void SetupRigidbodyDrop(Rigidbody rigidbody)
        {
            // Restore Rigidbody settings
            rigidbody.isKinematic = m_WasKinematic;
            rigidbody.useGravity = m_UsedGravity;
#if UNITY_2023_3_OR_NEWER
            rigidbody.linearDamping = m_OldLinearDamping;
            rigidbody.angularDamping = m_OldAngularDamping;
#else
            rigidbody.drag = m_OldLinearDamping;
            rigidbody.angularDrag = m_OldAngularDamping;
#endif

            if (!isSelected)
                m_Rigidbody.useGravity |= m_ForceGravityOnDetach;
        }

        void ResetThrowSmoothing()
        {
            Array.Clear(m_ThrowSmoothingFrameTimes, 0, m_ThrowSmoothingFrameTimes.Length);
            Array.Clear(m_ThrowSmoothingLinearVelocityFrames, 0, m_ThrowSmoothingLinearVelocityFrames.Length);
            Array.Clear(m_ThrowSmoothingAngularVelocityFrames, 0, m_ThrowSmoothingAngularVelocityFrames.Length);
            m_ThrowSmoothingCurrentFrame = 0;
            m_ThrowSmoothingFirstUpdate = true;
        }

        void EndThrowSmoothing()
        {
            if (m_ThrowOnDetach)
            {
                // This can be potentially improved for multi-hand throws by ignoring the frames
                // after the first interactor releases if the second interactor also releases within
                // a short period of time. Since the target pose is tracked before easing, the most
                // recent frames might have been a large change.
                var smoothedLinearVelocity = GetSmoothedVelocityValue(m_ThrowSmoothingLinearVelocityFrames);
                var smoothedAngularVelocity = GetSmoothedVelocityValue(m_ThrowSmoothingAngularVelocityFrames);
                m_DetachLinearVelocity = smoothedLinearVelocity * m_ThrowVelocityScale;
                m_DetachAngularVelocity = smoothedAngularVelocity * m_ThrowAngularVelocityScale;
            }
        }

        void StepThrowSmoothing(Pose targetPose, float deltaTime)
        {
            // Skip velocity calculations if Time.deltaTime is too low due to a frame-timing issue on Quest
            if (deltaTime < k_DeltaTimeThreshold)
                return;

            if (m_ThrowSmoothingFirstUpdate)
            {
                m_ThrowSmoothingFirstUpdate = false;
            }
            else
            {
                m_ThrowSmoothingLinearVelocityFrames[m_ThrowSmoothingCurrentFrame] = (targetPose.position - m_LastThrowReferencePose.position) / deltaTime;

                var rotationDiff = targetPose.rotation * Quaternion.Inverse(m_LastThrowReferencePose.rotation);
                var eulerAngles = rotationDiff.eulerAngles;
                var deltaAngles = new Vector3(Mathf.DeltaAngle(0f, eulerAngles.x),
                    Mathf.DeltaAngle(0f, eulerAngles.y),
                    Mathf.DeltaAngle(0f, eulerAngles.z));
                m_ThrowSmoothingAngularVelocityFrames[m_ThrowSmoothingCurrentFrame] = (deltaAngles / deltaTime) * Mathf.Deg2Rad;
            }

            m_ThrowSmoothingFrameTimes[m_ThrowSmoothingCurrentFrame] = Time.time;
            m_ThrowSmoothingCurrentFrame = (m_ThrowSmoothingCurrentFrame + 1) % k_ThrowSmoothingFrameCount;

            m_LastThrowReferencePose = targetPose;
        }

        Vector3 GetSmoothedVelocityValue(Vector3[] velocityFrames)
        {
            var calcVelocity = Vector3.zero;
            var totalWeights = 0f;
            for (var frameCounter = 0; frameCounter < k_ThrowSmoothingFrameCount; ++frameCounter)
            {
                var frameIdx = (((m_ThrowSmoothingCurrentFrame - frameCounter - 1) % k_ThrowSmoothingFrameCount) + k_ThrowSmoothingFrameCount) % k_ThrowSmoothingFrameCount;
                if (m_ThrowSmoothingFrameTimes[frameIdx] == 0f)
                    break;

                var timeAlpha = (Time.time - m_ThrowSmoothingFrameTimes[frameIdx]) / m_ThrowSmoothingDuration;
                var velocityWeight = m_ThrowSmoothingCurve.Evaluate(Mathf.Clamp(1f - timeAlpha, 0f, 1f));
                calcVelocity += velocityFrames[frameIdx] * velocityWeight;
                totalWeights += velocityWeight;
                if (Time.time - m_ThrowSmoothingFrameTimes[frameIdx] > m_ThrowSmoothingDuration)
                    break;
            }

            if (totalWeights > 0f)
                return calcVelocity / totalWeights;

            return Vector3.zero;
        }

        void SubscribeTeleportationProvider(IXRInteractor interactor)
        {
            m_TeleportationMonitor.AddInteractor(interactor);
        }

        void UnsubscribeTeleportationProvider(IXRInteractor interactor)
        {
            m_TeleportationMonitor.RemoveInteractor(interactor);
        }

        void OnTeleported(Pose offset)
        {
            var translated = offset.position;
            var rotated = offset.rotation;

            for (var frameIdx = 0; frameIdx < k_ThrowSmoothingFrameCount; ++frameIdx)
            {
                if (m_ThrowSmoothingFrameTimes[frameIdx] == 0f)
                    break;

                m_ThrowSmoothingLinearVelocityFrames[frameIdx] = rotated * m_ThrowSmoothingLinearVelocityFrames[frameIdx];
            }

            m_LastThrowReferencePose.position += translated;
            m_LastThrowReferencePose.rotation = rotated * m_LastThrowReferencePose.rotation;
        }

        void StartIgnoringCharacterCollision(Collider characterCollider)
        {
            m_IgnoringCharacterCollision = true;
            m_CollidersThatAllowedCharacterCollision.Clear();
            for (var index = 0; index < m_RigidbodyColliders.Count; ++index)
            {
                var rigidbodyCollider = m_RigidbodyColliders[index];
                if (rigidbodyCollider == null || rigidbodyCollider.isTrigger || Physics.GetIgnoreCollision(rigidbodyCollider, characterCollider))
                    continue;

                m_CollidersThatAllowedCharacterCollision.Add(rigidbodyCollider);
                Physics.IgnoreCollision(rigidbodyCollider, characterCollider, true);
            }
        }

        bool IsOutsideCharacterCollider(Collider characterCollider)
        {
            var characterBounds = characterCollider.bounds;
            foreach (var rigidbodyCollider in m_CollidersThatAllowedCharacterCollision)
            {
                if (rigidbodyCollider == null)
                    continue;

                if (rigidbodyCollider.bounds.Intersects(characterBounds))
                    return false;
            }

            return true;
        }

        void StopIgnoringCharacterCollision(Collider characterCollider)
        {
            m_IgnoringCharacterCollision = false;
            foreach (var rigidbodyCollider in m_CollidersThatAllowedCharacterCollision)
            {
                if (rigidbodyCollider != null)
                    Physics.IgnoreCollision(rigidbodyCollider, characterCollider, false);
            }
        }

        static Transform OnCreatePooledItem()
        {
            var item = new GameObject().transform;
            item.localPosition = Vector3.zero;
            item.localRotation = Quaternion.identity;
            item.localScale = Vector3.one;

            return item;
        }

        static void OnGetPooledItem(Transform item)
        {
            if (item == null)
                return;

            item.hideFlags &= ~HideFlags.HideInHierarchy;
        }

        static void OnReleasePooledItem(Transform item)
        {
            if (item == null)
                return;

            // Don't clear the parent of the GameObject on release since there could be issues
            // with changing it while a parent GameObject is deactivating, which logs an error.
            // By keeping it under this interactable, it could mean that GameObjects in the pool
            // have a chance of being destroyed, but we check that the GameObject we obtain from the pool
            // has not been destroyed. This means potentially more creations of new GameObjects, but avoids
            // the issue with reparenting.

            // Hide the GameObject in the Hierarchy so it doesn't pollute this Interactable's hierarchy
            // when it is no longer used.
            item.hideFlags |= HideFlags.HideInHierarchy;
        }

        static void OnDestroyPooledItem(Transform item)
        {
            if (item == null)
                return;

            Destroy(item.gameObject);
        }
    }
}
