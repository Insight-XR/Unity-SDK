using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR.Interaction.Toolkit.Utilities;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using Object = UnityEngine.Object;

namespace UnityEditor.XR.Interaction.Toolkit
{
    class GrabTransformersReorderableList : ReorderableList
    {
        public Action<IXRGrabTransformer, int> moveGrabTransformerTo { get; set; }

        public GrabTransformersReorderableList(IList elements)
            : base(elements, typeof(IXRGrabTransformer), true, false, false, false)
        {
            drawElementCallback += OnDrawListElement;
            onReorderCallbackWithDetails += OnReorderList;
            elementHeight = EditorGUIUtility.singleLineHeight;
            footerHeight = 0f;
        }

        void OnDrawListElement(Rect rect, int elementIndex, bool isActive, bool isFocused)
        {
            var element = list[elementIndex];
            rect.yMin += 1;
            EditorGUI.ObjectField(rect, $"Element {elementIndex}", element as Object, typeof(IXRGrabTransformer), true);
        }

        void OnReorderList(ReorderableList reorderableList, int oldIndex, int newIndex)
        {
            // The list has already been reordered when this callback is invoked,
            // so obtain the transform that was moved using the new index.
            if (list[newIndex] is IXRGrabTransformer transformer)
                moveGrabTransformerTo?.Invoke(transformer, newIndex);
        }
    }

    /// <summary>
    /// Custom editor for an <see cref="XRGrabInteractable"/>.
    /// </summary>
    [CustomEditor(typeof(XRGrabInteractable), true), CanEditMultipleObjects]
    public class XRGrabInteractableEditor : XRBaseInteractableEditor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.attachTransform"/>.</summary>
        protected SerializedProperty m_AttachTransform;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.secondaryAttachTransform"/>.</summary>
        protected SerializedProperty m_SecondaryAttachTransform;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.useDynamicAttach"/>.</summary>
        protected SerializedProperty m_UseDynamicAttach;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.matchAttachPosition"/>.</summary>
        protected SerializedProperty m_MatchAttachPosition;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.matchAttachRotation"/>.</summary>
        protected SerializedProperty m_MatchAttachRotation;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.snapToColliderVolume"/>.</summary>
        protected SerializedProperty m_SnapToColliderVolume;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.reinitializeDynamicAttachEverySingleGrab"/>.</summary>
        protected SerializedProperty m_ReinitializeDynamicAttachEverySingleGrab;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.attachEaseInTime"/>.</summary>
        protected SerializedProperty m_AttachEaseInTime;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.movementType"/>.</summary>
        protected SerializedProperty m_MovementType;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.velocityDamping"/>.</summary>
        protected SerializedProperty m_VelocityDamping;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.velocityScale"/>.</summary>
        protected SerializedProperty m_VelocityScale;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.angularVelocityDamping"/>.</summary>
        protected SerializedProperty m_AngularVelocityDamping;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.angularVelocityScale"/>.</summary>
        protected SerializedProperty m_AngularVelocityScale;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.trackPosition"/>.</summary>
        protected SerializedProperty m_TrackPosition;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothPosition"/>.</summary>
        protected SerializedProperty m_SmoothPosition;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothPositionAmount"/>.</summary>
        protected SerializedProperty m_SmoothPositionAmount;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.tightenPosition"/>.</summary>
        protected SerializedProperty m_TightenPosition;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.trackRotation"/>.</summary>
        protected SerializedProperty m_TrackRotation;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothRotation"/>.</summary>
        protected SerializedProperty m_SmoothRotation;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothRotationAmount"/>.</summary>
        protected SerializedProperty m_SmoothRotationAmount;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.tightenRotation"/>.</summary>
        protected SerializedProperty m_TightenRotation;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.trackScale"/>.</summary>
        protected SerializedProperty m_TrackScale;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothScale"/>.</summary>
        protected SerializedProperty m_SmoothScale;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.smoothScaleAmount"/>.</summary>
        protected SerializedProperty m_SmoothScaleAmount;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.tightenScale"/>.</summary>
        protected SerializedProperty m_TightenScale;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.throwOnDetach"/>.</summary>
        protected SerializedProperty m_ThrowOnDetach;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.throwSmoothingDuration"/>.</summary>
        protected SerializedProperty m_ThrowSmoothingDuration;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.throwSmoothingCurve"/>.</summary>
        protected SerializedProperty m_ThrowSmoothingCurve;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.throwVelocityScale"/>.</summary>
        protected SerializedProperty m_ThrowVelocityScale;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.throwAngularVelocityScale"/>.</summary>
        protected SerializedProperty m_ThrowAngularVelocityScale;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.forceGravityOnDetach"/>.</summary>
        protected SerializedProperty m_ForceGravityOnDetach;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.retainTransformParent"/>.</summary>
        protected SerializedProperty m_RetainTransformParent;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.attachPointCompatibilityMode"/>.</summary>
        protected SerializedProperty m_AttachPointCompatibilityMode;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.addDefaultGrabTransformers"/>.</summary>
        protected SerializedProperty m_AddDefaultGrabTransformers;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.startingMultipleGrabTransformers"/>.</summary>
        protected SerializedProperty m_StartingMultipleGrabTransformers;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGrabInteractable.startingSingleGrabTransformers"/>.</summary>
        protected SerializedProperty m_StartingSingleGrabTransformers;

        /// <summary>Value to be checked before recalculate if the inspected object has a non-uniformly scaled parent.</summary>
        bool m_RecalculateHasNonUniformScale = true;
        /// <summary>Caches if the inspected object has a non-uniformly scaled parent.</summary>
        bool m_HasNonUniformScale;

        List<IXRGrabTransformer> m_SingleGrabTransformers;
        List<IXRGrabTransformer> m_MultipleGrabTransformers;
        GrabTransformersReorderableList m_SingleGrabTransformersReorderableList;
        GrabTransformersReorderableList m_MultipleGrabTransformersReorderableList;

        bool m_SingleGrabTransformersExpanded = true;
        bool m_MultipleGrabTransformersExpanded = true;

        const string k_SingleGrabTransformersExpandedKey = "XRI." + nameof(XRGrabInteractableEditor) + ".SingleGrabTransformersExpanded";
        const string k_MultipleGrabTransformersExpandedKey = "XRI." + nameof(XRGrabInteractableEditor) + ".MultipleGrabTransformersExpanded";

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.attachTransform"/>.</summary>
            public static readonly GUIContent attachTransform = EditorGUIUtility.TrTextContent("Attach Transform", "The attachment point Unity uses on this Interactable (will use this object's position if none set).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.secondaryAttachTransform"/>.</summary>
            public static readonly GUIContent secondaryAttachTransform = EditorGUIUtility.TrTextContent("Secondary Attach Transform", "The secondary attachment point to use on this Interactable for multi-hand interaction (will use the second interactor's attach transform if none set).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.useDynamicAttach"/>.</summary>
            public static readonly GUIContent useDynamicAttach = EditorGUIUtility.TrTextContent("Use Dynamic Attach", "Enable to make the effective attachment point based on the pose of the Interactor when the selection is made.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.matchAttachPosition"/>.</summary>
            public static readonly GUIContent matchAttachPosition = EditorGUIUtility.TrTextContent("Match Position", "Match the position of the Interactor's attachment point when initializing the grab. This will override the position of Attach Transform.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.matchAttachRotation"/>.</summary>
            public static readonly GUIContent matchAttachRotation = EditorGUIUtility.TrTextContent("Match Rotation", "Match the rotation of the Interactor's attachment point when initializing the grab. This will override the rotation of Attach Transform.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.snapToColliderVolume"/>.</summary>
            public static readonly GUIContent snapToColliderVolume = EditorGUIUtility.TrTextContent("Snap To Collider Volume", "Adjust the dynamic attachment point to keep it on or inside the Colliders that make up this object.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.reinitializeDynamicAttachEverySingleGrab"/>.</summary>
            public static readonly GUIContent reinitializeDynamicAttachEverySingleGrab = EditorGUIUtility.TrTextContent("Reinitialize Every Single Grab", "Re-initialize the dynamic pose when changing from multiple grabs back to a single grab.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.attachEaseInTime"/>.</summary>
            public static readonly GUIContent attachEaseInTime = EditorGUIUtility.TrTextContent("Attach Ease In Time", "Time in seconds to ease in the attach when selected (a value of 0 indicates no easing).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.movementType"/>.</summary>
            public static readonly GUIContent movementType = EditorGUIUtility.TrTextContent("Movement Type", "Specifies how this object is moved when selected, either through setting the velocity of the Rigidbody, moving the kinematic Rigidbody during Fixed Update, or by directly updating the Transform each frame.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.velocityDamping"/>.</summary>
            public static readonly GUIContent velocityDamping = EditorGUIUtility.TrTextContent("Velocity Damping", "Scale factor of how much to dampen the existing linear velocity when tracking the position of the Interactor. The smaller the value, the longer it takes for the velocity to decay.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.velocityScale"/>.</summary>
            public static readonly GUIContent velocityScale = EditorGUIUtility.TrTextContent("Velocity Scale", "Scale factor applied to the tracked linear velocity while updating the Rigidbody when tracking the position of the Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.angularVelocityDamping"/>.</summary>
            public static readonly GUIContent angularVelocityDamping = EditorGUIUtility.TrTextContent("Angular Velocity Damping", "Scale factor of how much to dampen the existing angular velocity when tracking the rotation of the Interactor. The smaller the value, the longer it takes for the angular velocity to decay.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.angularVelocityScale"/>.</summary>
            public static readonly GUIContent angularVelocityScale = EditorGUIUtility.TrTextContent("Angular Velocity Scale", "Scale factor applied to the tracked angular velocity while updating the Rigidbody when tracking the rotation of the Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.trackPosition"/>.</summary>
            public static readonly GUIContent trackPosition = EditorGUIUtility.TrTextContent("Track Position", "Whether this object should follow the position of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothPosition"/>.</summary>
            public static readonly GUIContent smoothPosition = EditorGUIUtility.TrTextContent("Smooth Position", "Apply smoothing while following the position of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothPositionAmount"/>.</summary>
            public static readonly GUIContent smoothPositionAmount = EditorGUIUtility.TrTextContent("Smooth Position Amount", "Scale factor for how much smoothing is applied while following the position of the Interactor when selected. The larger the value, the closer this object will remain to the position of the Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.tightenPosition"/>.</summary>
            public static readonly GUIContent tightenPosition = EditorGUIUtility.TrTextContent("Tighten Position", "Reduces the maximum follow position difference when using smoothing. The value ranges from 0 meaning no bias in the smoothed follow distance, to 1 meaning effectively no smoothing at all.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.trackRotation"/>.</summary>
            public static readonly GUIContent trackRotation = EditorGUIUtility.TrTextContent("Track Rotation", "Whether this object should follow the rotation of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothRotation"/>.</summary>
            public static readonly GUIContent smoothRotation = EditorGUIUtility.TrTextContent("Smooth Rotation", "Apply smoothing while following the rotation of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothRotationAmount"/>.</summary>
            public static readonly GUIContent smoothRotationAmount = EditorGUIUtility.TrTextContent("Smooth Rotation Amount", "Scale factor for how much smoothing is applied while following the rotation of the Interactor when selected. The larger the value, the closer this object will remain to the rotation of the Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.tightenRotation"/>.</summary>
            public static readonly GUIContent tightenRotation = EditorGUIUtility.TrTextContent("Tighten Rotation", "Reduces the maximum follow rotation difference when using smoothing. The value ranges from 0 meaning no bias in the smoothed follow rotation, to 1 meaning effectively no smoothing at all.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.trackScale"/>.</summary>
            public static readonly GUIContent trackScale = EditorGUIUtility.TrTextContent("Track Scale", "Whether this object should follow the scale of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothScale"/>.</summary>
            public static readonly GUIContent smoothScale = EditorGUIUtility.TrTextContent("Smooth Scale", "Apply smoothing while following the scale of the Interactor when selected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.smoothScaleAmount"/>.</summary>
            public static readonly GUIContent smoothScaleAmount = EditorGUIUtility.TrTextContent("Smooth Scale Amount", "Scale factor for how much smoothing is applied while following the scale of the interactable when selected. The larger the value, the closer this object will remain to the target scale determined by the interactable's transformer.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.tightenScale"/>.</summary>
            public static readonly GUIContent tightenScale = EditorGUIUtility.TrTextContent("Tighten Scale", "Reduces the maximum follow scale difference when using smoothing. The value ranges from 0 meaning no bias in the smoothed follow scale, to 1 meaning effectively no smoothing at all.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.throwOnDetach"/>.</summary>
            public static readonly GUIContent throwOnDetach = EditorGUIUtility.TrTextContent("Throw On Detach", "Whether this object inherits the velocity of the Interactor when released. This is not supported for a kinematic Rigidbody.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.throwSmoothingDuration"/>.</summary>
            public static readonly GUIContent throwSmoothingDuration = EditorGUIUtility.TrTextContent("Throw Smoothing Duration", "This value represents the time over which collected samples are used for velocity calculation, up to a max of 20 previous frames.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.throwSmoothingCurve"/>.</summary>
            public static readonly GUIContent throwSmoothingCurve = EditorGUIUtility.TrTextContent("Throw Smoothing Curve", "The curve to use to weight thrown velocity smoothing (most recent frames to the right).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.throwVelocityScale"/>.</summary>
            public static readonly GUIContent throwVelocityScale = EditorGUIUtility.TrTextContent("Throw Velocity Scale", "Scale factor applied to this object's inherited linear velocity of the Interactor when released.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.throwAngularVelocityScale"/>.</summary>
            public static readonly GUIContent throwAngularVelocityScale = EditorGUIUtility.TrTextContent("Throw Angular Velocity Scale", "Scale factor applied to this object's inherited angular velocity of the Interactor when released.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.forceGravityOnDetach"/>.</summary>
            public static readonly GUIContent forceGravityOnDetach = EditorGUIUtility.TrTextContent("Force Gravity On Detach", "Force this object to have gravity when released (will still use pre-grab value if this is false).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.retainTransformParent"/>.</summary>
            public static readonly GUIContent retainTransformParent = EditorGUIUtility.TrTextContent("Retain Transform Parent", "Whether to set the parent of this object back to its original parent this object was a child of after this object is dropped.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.attachPointCompatibilityMode"/>.</summary>
            public static readonly GUIContent attachPointCompatibilityMode = EditorGUIUtility.TrTextContent("Attach Point Compatibility Mode", "Use Default for consistent attach points between all Movement Type values. Use Legacy for older projects that want to maintain the incorrect method which was partially based on center of mass.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.addDefaultGrabTransformers"/>.</summary>
            public static readonly GUIContent addDefaultGrabTransformers = EditorGUIUtility.TrTextContent("Add Default Grab Transformers", "Whether Unity will add the default set of grab transformers if either the Single or Multiple Grab Transformers lists are empty.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.startingMultipleGrabTransformers"/>.</summary>
            public static readonly GUIContent startingMultipleGrabTransformers = EditorGUIUtility.TrTextContent("Starting Multiple Grab Transformers", "The grab transformers that this Interactable automatically links at startup (optional, may be empty). Used for multi-interactor selection.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGrabInteractable.startingSingleGrabTransformers"/>.</summary>
            public static readonly GUIContent startingSingleGrabTransformers = EditorGUIUtility.TrTextContent("Starting Single Grab Transformers", "The grab transformers that this Interactable automatically links at startup (optional, may be empty). Used for single-interactor selection.");

            /// <summary><see cref="GUIContent"/> for the Multiple Grab Transformers list in Play mode.</summary>
            public static readonly GUIContent grabTransformersConfiguration = EditorGUIUtility.TrTextContent("Grab Transformers Configuration");
            /// <summary><see cref="GUIContent"/> for the Multiple Grab Transformers list in Play mode.</summary>
            public static readonly GUIContent multipleGrabTransformers = EditorGUIUtility.TrTextContent("Multiple Grab Transformers", "The grab transformers used when there are multiple interactors selecting this object.");
            /// <summary><see cref="GUIContent"/> for the Single Grab Transformers list in Play mode.</summary>
            public static readonly GUIContent singleGrabTransformers = EditorGUIUtility.TrTextContent("Single Grab Transformers", "The grab transformers used when there is a single interactor selecting this object.");

            /// <summary>Message for non-uniformly scaled parent.</summary>
            public static readonly string nonUniformScaledParentWarning = "When a child object has a non-uniformly scaled parent and is rotated relative to that parent, it may appear skewed. To avoid this, use uniform scale in all parents' Transform of this object.";

            /// <summary>Array of type <see cref="GUIContent"/> for the options shown in the popup for <see cref="XRGrabInteractable.attachPointCompatibilityMode"/>.</summary>
            public static readonly GUIContent[] attachPointCompatibilityModeOptions =
            {
                EditorGUIUtility.TrTextContent("Default (Recommended)"),
                EditorGUIUtility.TrTextContent("Legacy (Obsolete)")
            };
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            m_AttachTransform = serializedObject.FindProperty("m_AttachTransform");
            m_SecondaryAttachTransform = serializedObject.FindProperty("m_SecondaryAttachTransform");
            m_UseDynamicAttach = serializedObject.FindProperty("m_UseDynamicAttach");
            m_MatchAttachPosition = serializedObject.FindProperty("m_MatchAttachPosition");
            m_MatchAttachRotation = serializedObject.FindProperty("m_MatchAttachRotation");
            m_SnapToColliderVolume = serializedObject.FindProperty("m_SnapToColliderVolume");
            m_ReinitializeDynamicAttachEverySingleGrab = serializedObject.FindProperty("m_ReinitializeDynamicAttachEverySingleGrab");
            m_AttachEaseInTime = serializedObject.FindProperty("m_AttachEaseInTime");
            m_MovementType = serializedObject.FindProperty("m_MovementType");
            m_VelocityDamping = serializedObject.FindProperty("m_VelocityDamping");
            m_VelocityScale = serializedObject.FindProperty("m_VelocityScale");
            m_AngularVelocityDamping = serializedObject.FindProperty("m_AngularVelocityDamping");
            m_AngularVelocityScale = serializedObject.FindProperty("m_AngularVelocityScale");
            m_TrackPosition = serializedObject.FindProperty("m_TrackPosition");
            m_SmoothPosition = serializedObject.FindProperty("m_SmoothPosition");
            m_SmoothPositionAmount = serializedObject.FindProperty("m_SmoothPositionAmount");
            m_TightenPosition = serializedObject.FindProperty("m_TightenPosition");
            m_TrackRotation = serializedObject.FindProperty("m_TrackRotation");
            m_SmoothRotation = serializedObject.FindProperty("m_SmoothRotation");
            m_SmoothRotationAmount = serializedObject.FindProperty("m_SmoothRotationAmount");
            m_TightenRotation = serializedObject.FindProperty("m_TightenRotation");
            m_TrackScale = serializedObject.FindProperty("m_TrackScale");
            m_SmoothScale = serializedObject.FindProperty("m_SmoothScale");
            m_SmoothScaleAmount = serializedObject.FindProperty("m_SmoothScaleAmount");
            m_TightenScale = serializedObject.FindProperty("m_TightenScale");
            m_ThrowOnDetach = serializedObject.FindProperty("m_ThrowOnDetach");
            m_ThrowSmoothingDuration = serializedObject.FindProperty("m_ThrowSmoothingDuration");
            m_ThrowSmoothingCurve = serializedObject.FindProperty("m_ThrowSmoothingCurve");
            m_ThrowVelocityScale = serializedObject.FindProperty("m_ThrowVelocityScale");
            m_ThrowAngularVelocityScale = serializedObject.FindProperty("m_ThrowAngularVelocityScale");
            m_ForceGravityOnDetach = serializedObject.FindProperty("m_ForceGravityOnDetach");
            m_RetainTransformParent = serializedObject.FindProperty("m_RetainTransformParent");
            m_AttachPointCompatibilityMode = serializedObject.FindProperty("m_AttachPointCompatibilityMode");
            m_AddDefaultGrabTransformers = serializedObject.FindProperty("m_AddDefaultGrabTransformers");
            m_StartingMultipleGrabTransformers = serializedObject.FindProperty("m_StartingMultipleGrabTransformers");
            m_StartingSingleGrabTransformers = serializedObject.FindProperty("m_StartingSingleGrabTransformers");

            m_SingleGrabTransformers = new List<IXRGrabTransformer>();
            m_SingleGrabTransformersReorderableList = new GrabTransformersReorderableList(m_SingleGrabTransformers)
            {
                moveGrabTransformerTo = ((XRGrabInteractable)target).MoveSingleGrabTransformerTo,
            };
            m_MultipleGrabTransformers = new List<IXRGrabTransformer>();
            m_MultipleGrabTransformersReorderableList = new GrabTransformersReorderableList(m_MultipleGrabTransformers)
            {
                moveGrabTransformerTo = ((XRGrabInteractable)target).MoveMultipleGrabTransformerTo,
            };

            m_SingleGrabTransformersExpanded = SessionState.GetBool(k_SingleGrabTransformersExpandedKey, true);
            m_MultipleGrabTransformersExpanded = SessionState.GetBool(k_MultipleGrabTransformersExpandedKey, true);

            Undo.postprocessModifications += OnPostprocessModifications;
        }

        /// <summary>
        /// This function is called when the object becomes disabled.
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>
        protected virtual void OnDisable()
        {
            SessionState.SetBool(k_SingleGrabTransformersExpandedKey, m_SingleGrabTransformersExpanded);
            SessionState.SetBool(k_MultipleGrabTransformersExpandedKey, m_MultipleGrabTransformersExpanded);

            Undo.postprocessModifications -= OnPostprocessModifications;
        }

        /// <inheritdoc />
        protected override void DrawProperties()
        {
            base.DrawProperties();

            EditorGUILayout.Space();

            DrawGrabConfiguration();
            DrawTrackConfiguration();
            DrawDetachConfiguration();
            DrawAttachConfiguration();
            DrawGrabTransformersConfiguration();
        }

        /// <summary>
        /// Draw the property fields related to grab configuration.
        /// </summary>
        protected virtual void DrawGrabConfiguration()
        {
            EditorGUILayout.PropertyField(m_MovementType, Contents.movementType);
            EditorGUILayout.PropertyField(m_RetainTransformParent, Contents.retainTransformParent);
            DrawNonUniformScaleMessage();
        }

        /// <summary>
        /// Draw the property fields related to tracking configuration.
        /// </summary>
        protected virtual void DrawTrackConfiguration()
        {
            EditorGUILayout.PropertyField(m_TrackPosition, Contents.trackPosition);
            if (m_TrackPosition.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_SmoothPosition, Contents.smoothPosition);
                    if (m_SmoothPosition.boolValue)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.PropertyField(m_SmoothPositionAmount, Contents.smoothPositionAmount);
                            EditorGUILayout.PropertyField(m_TightenPosition, Contents.tightenPosition);
                        }
                    }

                    if (m_MovementType.intValue == (int)XRBaseInteractable.MovementType.VelocityTracking)
                    {
                        EditorGUILayout.PropertyField(m_VelocityDamping, Contents.velocityDamping);
                        EditorGUILayout.PropertyField(m_VelocityScale, Contents.velocityScale);
                    }
                }
            }

            EditorGUILayout.PropertyField(m_TrackRotation, Contents.trackRotation);
            if (m_TrackRotation.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_SmoothRotation, Contents.smoothRotation);
                    if (m_SmoothRotation.boolValue)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.PropertyField(m_SmoothRotationAmount, Contents.smoothRotationAmount);
                            EditorGUILayout.PropertyField(m_TightenRotation, Contents.tightenRotation);
                        }
                    }

                    if (m_MovementType.intValue == (int)XRBaseInteractable.MovementType.VelocityTracking)
                    {
                        EditorGUILayout.PropertyField(m_AngularVelocityDamping, Contents.angularVelocityDamping);
                        EditorGUILayout.PropertyField(m_AngularVelocityScale, Contents.angularVelocityScale);
                    }
                }
            }
            
            EditorGUILayout.PropertyField(m_TrackScale, Contents.trackScale);
            
            if (m_TrackScale.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_SmoothScale, Contents.smoothScale);
                    if (m_SmoothScale.boolValue)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.PropertyField(m_SmoothScaleAmount, Contents.smoothScaleAmount);
                            EditorGUILayout.PropertyField(m_TightenScale, Contents.tightenScale);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw property fields related to detach configuration.
        /// </summary>
        protected virtual void DrawDetachConfiguration()
        {
            EditorGUILayout.PropertyField(m_ThrowOnDetach, Contents.throwOnDetach);
            if (m_ThrowOnDetach.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_ThrowSmoothingDuration, Contents.throwSmoothingDuration);
                    EditorGUILayout.PropertyField(m_ThrowSmoothingCurve, Contents.throwSmoothingCurve);
                    EditorGUILayout.PropertyField(m_ThrowVelocityScale, Contents.throwVelocityScale);
                    EditorGUILayout.PropertyField(m_ThrowAngularVelocityScale, Contents.throwAngularVelocityScale);
                }
            }

            EditorGUILayout.PropertyField(m_ForceGravityOnDetach, Contents.forceGravityOnDetach);
        }

        /// <summary>
        /// Draw property fields related to attach configuration.
        /// </summary>
        protected virtual void DrawAttachConfiguration()
        {
            EditorGUILayout.PropertyField(m_AttachTransform, Contents.attachTransform);
            EditorGUILayout.PropertyField(m_SecondaryAttachTransform, Contents.secondaryAttachTransform);
            EditorGUILayout.PropertyField(m_UseDynamicAttach, Contents.useDynamicAttach);
            if (m_UseDynamicAttach.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_MatchAttachPosition, Contents.matchAttachPosition);
                    EditorGUILayout.PropertyField(m_MatchAttachRotation, Contents.matchAttachRotation);
                    using (new EditorGUI.DisabledScope(!m_MatchAttachPosition.boolValue))
                    {
                        EditorGUILayout.PropertyField(m_SnapToColliderVolume, Contents.snapToColliderVolume);
                    }

                    EditorGUILayout.PropertyField(m_ReinitializeDynamicAttachEverySingleGrab, Contents.reinitializeDynamicAttachEverySingleGrab);
                }
            }

            EditorGUILayout.PropertyField(m_AttachEaseInTime, Contents.attachEaseInTime);
            XRInteractionEditorGUI.EnumPropertyField(m_AttachPointCompatibilityMode, Contents.attachPointCompatibilityMode, Contents.attachPointCompatibilityModeOptions);
        }

        /// <summary>
        /// Draw the Grab Transformers Configuration foldout.
        /// </summary>
        protected virtual void DrawGrabTransformersConfiguration()
        {
            m_AddDefaultGrabTransformers.isExpanded = EditorGUILayout.Foldout(m_AddDefaultGrabTransformers.isExpanded, Contents.grabTransformersConfiguration, true);
            if (m_AddDefaultGrabTransformers.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawGrabTransformersConfigurationNested();
                }
            }
        }

        /// <summary>
        /// Draw property fields related to grab transformers.
        /// </summary>
        protected virtual void DrawGrabTransformersConfigurationNested()
        {
            using (new EditorGUI.DisabledScope(m_AttachPointCompatibilityMode.intValue == (int)XRGrabInteractable.AttachPointCompatibilityMode.Legacy))
            {
                EditorGUILayout.PropertyField(m_AddDefaultGrabTransformers, Contents.addDefaultGrabTransformers);

                using (new EditorGUI.DisabledScope(Application.isPlaying))
                {
                    EditorGUILayout.PropertyField(m_StartingMultipleGrabTransformers, Contents.startingMultipleGrabTransformers);
                    EditorGUILayout.PropertyField(m_StartingSingleGrabTransformers, Contents.startingSingleGrabTransformers);
                }
            }

            if (Application.isPlaying)
            {
                if (serializedObject.isEditingMultipleObjects)
                {
                    EditorGUILayout.HelpBox("Grab Transformers cannot be multi-edited.", MessageType.None);
                }
                else
                {
                    var grabInteractable = (XRGrabInteractable)target;
                    m_MultipleGrabTransformersExpanded = EditorGUILayout.Foldout(m_MultipleGrabTransformersExpanded, Contents.multipleGrabTransformers, true);
                    if (m_MultipleGrabTransformersExpanded)
                    {
                        grabInteractable.GetMultipleGrabTransformers(m_MultipleGrabTransformers);
                        m_MultipleGrabTransformersReorderableList.DoLayoutList();
                    }

                    m_SingleGrabTransformersExpanded = EditorGUILayout.Foldout(m_SingleGrabTransformersExpanded, Contents.singleGrabTransformers, true);
                    if (m_SingleGrabTransformersExpanded)
                    {
                        grabInteractable.GetSingleGrabTransformers(m_SingleGrabTransformers);
                        m_SingleGrabTransformersReorderableList.DoLayoutList();
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the object has a non-uniformly scaled parent and draws a message if necessary.
        /// </summary>
        protected virtual void DrawNonUniformScaleMessage()
        {
            if (m_RetainTransformParent == null || !m_RetainTransformParent.boolValue)
                return;

            if (m_RecalculateHasNonUniformScale)
            {
                var monoBehaviour = target as MonoBehaviour;
                if (monoBehaviour == null)
                    return;

                var transform = monoBehaviour.transform;
                if (transform == null)
                    return;

                m_HasNonUniformScale = false;
                for (var parent = transform.parent; parent != null; parent = parent.parent)
                {
                    var localScale = parent.localScale;
                    if (!Mathf.Approximately(localScale.x, localScale.y) ||
                        !Mathf.Approximately(localScale.x, localScale.z))
                    {
                        m_HasNonUniformScale = true;
                        break;
                    }
                }

                m_RecalculateHasNonUniformScale = false;
            }

            if (m_HasNonUniformScale)
                EditorGUILayout.HelpBox(Contents.nonUniformScaledParentWarning, MessageType.Warning);
        }

        /// <summary>
        /// Callback registered to be triggered whenever a new set of property modifications is created.
        /// </summary>
        /// <seealso cref="Undo.postprocessModifications"/>
        protected virtual UndoPropertyModification[] OnPostprocessModifications(UndoPropertyModification[] modifications)
        {
            m_RecalculateHasNonUniformScale = true;
            return modifications;
        }
    }
}
