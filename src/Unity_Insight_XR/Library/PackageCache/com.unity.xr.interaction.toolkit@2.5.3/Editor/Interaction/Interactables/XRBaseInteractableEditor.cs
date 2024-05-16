using System;
using System.Collections.Generic;
using UnityEditor.XR.Interaction.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="XRBaseInteractable"/>.
    /// </summary>
    [CustomEditor(typeof(XRBaseInteractable), true), CanEditMultipleObjects]
    public partial class XRBaseInteractableEditor : BaseInteractionEditor
    {
        const string k_GazeConfigurationExpandedKey = "XRI." + nameof(XRBaseInteractableEditor) + ".GazeConfigurationExpanded";
        const string k_FiltersExpandedKey = "XRI." + nameof(XRBaseInteractableEditor) + ".FiltersExpanded";
        const string k_HoverFiltersExpandedKey = "XRI." + nameof(XRBaseInteractableEditor) + ".HoverFiltersExpanded";
        const string k_SelectFiltersExpandedKey = "XRI." + nameof(XRBaseInteractableEditor) + ".SelectFiltersExpanded";
        const string k_InteractionStrengthFiltersExpandedKey = "XRI." + nameof(XRBaseInteractableEditor) + ".InteractionStrengthFiltersExpanded";

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.interactionManager"/>.</summary>
        protected SerializedProperty m_InteractionManager;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.colliders"/>.</summary>
        protected SerializedProperty m_Colliders;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.interactionLayerMask"/>.</summary>
        protected SerializedProperty m_InteractionLayerMask;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.interactionLayers"/>.</summary>
        protected SerializedProperty m_InteractionLayers;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.distanceCalculationMode"/>.</summary>
        protected SerializedProperty m_DistanceCalculationMode;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.selectMode"/>.</summary>
        protected SerializedProperty m_SelectMode;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.focusMode"/>.</summary>
        protected SerializedProperty m_FocusMode;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.startingHoverFilters"/>.</summary>
        protected SerializedProperty m_StartingHoverFilters;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.startingSelectFilters"/>.</summary>
        protected SerializedProperty m_StartingSelectFilters;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.startingInteractionStrengthFilters"/>.</summary>
        protected SerializedProperty m_StartingInteractionStrengthFilters;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.customReticle"/>.</summary>
        protected SerializedProperty m_CustomReticle;

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.allowGazeInteraction"/>.</summary>
        protected SerializedProperty m_AllowGazeInteraction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.allowGazeSelect"/>.</summary>
        protected SerializedProperty m_AllowGazeSelect;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.overrideGazeTimeToSelect"/>.</summary>
        protected SerializedProperty m_OverrideGazeTimeToSelect;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.gazeTimeToSelect"/>.</summary>
        protected SerializedProperty m_GazeTimeToSelect;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.allowGazeAssistance"/>.</summary>
        protected SerializedProperty m_AllowGazeAssistance;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.overrideTimeToAutoDeselectGaze"/>.</summary>
        protected SerializedProperty m_OverrideTimeToAutoDeselectGaze;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.timeToAutoDeselectGaze"/>.</summary>
        protected SerializedProperty m_TimeToAutoDeselectGaze;

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.firstHoverEntered"/>.</summary>
        protected SerializedProperty m_FirstHoverEntered;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.lastHoverExited"/>.</summary>
        protected SerializedProperty m_LastHoverExited;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.hoverEntered"/>.</summary>
        protected SerializedProperty m_HoverEntered;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.hoverExited"/>.</summary>
        protected SerializedProperty m_HoverExited;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.firstSelectEntered"/>.</summary>
        protected SerializedProperty m_FirstSelectEntered;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.lastSelectExited"/>.</summary>
        protected SerializedProperty m_LastSelectExited;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.selectEntered"/>.</summary>
        protected SerializedProperty m_SelectEntered;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.selectExited"/>.</summary>
        protected SerializedProperty m_SelectExited;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.firstFocusEntered"/>.</summary>
        protected SerializedProperty m_FirstFocusEntered;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.lastFocusExited"/>.</summary>
        protected SerializedProperty m_LastFocusExited;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.focusEntered"/>.</summary>
        protected SerializedProperty m_FocusEntered;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.focusExited"/>.</summary>
        protected SerializedProperty m_FocusExited;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.activated"/>.</summary>
        protected SerializedProperty m_Activated;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.deactivated"/>.</summary>
        protected SerializedProperty m_Deactivated;

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.onFirstHoverEntered"/>.</summary>
        protected SerializedProperty m_OnFirstHoverEntered;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.onLastHoverExited"/>.</summary>
        protected SerializedProperty m_OnLastHoverExited;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.onHoverEntered"/>.</summary>
        protected SerializedProperty m_OnHoverEntered;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.onHoverExited"/>.</summary>
        protected SerializedProperty m_OnHoverExited;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.onSelectEntered"/>.</summary>
        protected SerializedProperty m_OnSelectEntered;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.onSelectExited"/>.</summary>
        protected SerializedProperty m_OnSelectExited;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.onSelectCanceled"/>.</summary>
        protected SerializedProperty m_OnSelectCanceled;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.onActivate"/>.</summary>
        protected SerializedProperty m_OnActivate;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRBaseInteractable.onDeactivate"/>.</summary>
        protected SerializedProperty m_OnDeactivate;

        /// <summary><see cref="SerializedProperty"/> of the persistent calls backing <see cref="XRBaseInteractable.onFirstHoverEntered"/>.</summary>
        protected SerializedProperty m_OnFirstHoverEnteredCalls;
        /// <summary><see cref="SerializedProperty"/> of the persistent calls backing <see cref="XRBaseInteractable.onLastHoverExited"/>.</summary>
        protected SerializedProperty m_OnLastHoverExitedCalls;
        /// <summary><see cref="SerializedProperty"/> of the persistent calls backing <see cref="XRBaseInteractable.onHoverEntered"/>.</summary>
        protected SerializedProperty m_OnHoverEnteredCalls;
        /// <summary><see cref="SerializedProperty"/> of the persistent calls backing <see cref="XRBaseInteractable.onHoverExited"/>.</summary>
        protected SerializedProperty m_OnHoverExitedCalls;
        /// <summary><see cref="SerializedProperty"/> of the persistent calls backing <see cref="XRBaseInteractable.onSelectEntered"/>.</summary>
        protected SerializedProperty m_OnSelectEnteredCalls;
        /// <summary><see cref="SerializedProperty"/> of the persistent calls backing <see cref="XRBaseInteractable.onSelectExited"/>.</summary>
        protected SerializedProperty m_OnSelectExitedCalls;
        /// <summary><see cref="SerializedProperty"/> of the persistent calls backing <see cref="XRBaseInteractable.onSelectCanceled"/>.</summary>
        protected SerializedProperty m_OnSelectCanceledCalls;
        /// <summary><see cref="SerializedProperty"/> of the persistent calls backing <see cref="XRBaseInteractable.onActivate"/>.</summary>
        protected SerializedProperty m_OnActivateCalls;
        /// <summary><see cref="SerializedProperty"/> of the persistent calls backing <see cref="XRBaseInteractable.onDeactivate"/>.</summary>
        protected SerializedProperty m_OnDeactivateCalls;

        bool m_GazeConfigurationExpanded;
        bool m_FiltersExpanded;
        ReadOnlyReorderableList<IXRHoverFilter> m_HoverFilters;
        ReadOnlyReorderableList<IXRSelectFilter> m_SelectFilters;
        ReadOnlyReorderableList<IXRInteractionStrengthFilter> m_InteractionStrengthFilters;

        /// <summary>
        /// Whether <see cref="InteractableSelectMode.Multiple"/> is allowed by the script of the object being inspected.
        /// </summary>
        protected bool selectMultipleAllowed { get; private set; }

        /// <summary>
        /// Whether <see cref="InteractableFocusMode.Multiple"/> is allowed by the script of the object being inspected.
        /// </summary>
        protected bool focusMultipleAllowed { get; private set; }

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class BaseContents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.interactionManager"/>.</summary>
            public static readonly GUIContent interactionManager = EditorGUIUtility.TrTextContent("Interaction Manager", "The XR Interaction Manager that this Interactable will communicate with (will find one if None).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.colliders"/>.</summary>
            public static readonly GUIContent colliders = EditorGUIUtility.TrTextContent("Colliders", "Colliders to include when selecting/interacting with this Interactable (if empty, will use any child Colliders).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.interactionLayerMask"/>.</summary>
            public static readonly GUIContent interactionLayerMask = EditorGUIUtility.TrTextContent("Deprecated Interaction Layer Mask", "Deprecated Interaction Layer Mask that uses the Unity physics Layers. Hide this property by disabling \'Show Old Interaction Layer Mask In Inspector\' in the XR Interaction Toolkit project settings.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.interactionLayers"/>.</summary>
            public static readonly GUIContent interactionLayers = EditorGUIUtility.TrTextContent("Interaction Layer Mask", "Allows interaction with Interactors whose Interaction Layer Mask overlaps with any Layer in this Interaction Layer Mask.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.selectMode"/>.</summary>
            public static readonly GUIContent selectMode = EditorGUIUtility.TrTextContent("Select Mode", "The selection policy, either Single selection with swapping allowed, or Multiple selection.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.focusMode"/>.</summary>
            public static readonly GUIContent focusMode = EditorGUIUtility.TrTextContent("Focus Mode", "The focus policy, either Single focus with swapping allowed, Multiple focus, or None to disallow focus");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.distanceCalculationMode"/>.</summary>
            public static readonly GUIContent distanceCalculationMode = EditorGUIUtility.TrTextContent("Distance Calculation Mode", "Specifies how distance is calculated to Interactors, from fastest to most accurate. If using Mesh Colliders, Collider Volume only works if the mesh is convex.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.customReticle"/>.</summary>
            public static readonly GUIContent customReticle = EditorGUIUtility.TrTextContent("Custom Reticle", "The reticle that will appear at the end of the line when it is valid.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.onFirstHoverEntered"/>.</summary>
            public static readonly GUIContent onFirstHoverEntered = EditorGUIUtility.TrTextContent("(Deprecated) On First Hover Entered");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.onLastHoverExited"/>.</summary>
            public static readonly GUIContent onLastHoverExited = EditorGUIUtility.TrTextContent("(Deprecated) On Last Hover Exited");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.onHoverEntered"/>.</summary>
            public static readonly GUIContent onHoverEntered = EditorGUIUtility.TrTextContent("(Deprecated) On Hover Entered");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.onHoverExited"/>.</summary>
            public static readonly GUIContent onHoverExited = EditorGUIUtility.TrTextContent("(Deprecated) On Hover Exited");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.onSelectEntered"/>.</summary>
            public static readonly GUIContent onSelectEntered = EditorGUIUtility.TrTextContent("(Deprecated) On Select Entered");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.onSelectExited"/>.</summary>
            public static readonly GUIContent onSelectExited = EditorGUIUtility.TrTextContent("(Deprecated) On Select Exited");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.onSelectCanceled"/>.</summary>
            public static readonly GUIContent onSelectCanceled = EditorGUIUtility.TrTextContent("(Deprecated) On Select Canceled");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.onActivate"/>.</summary>
            public static readonly GUIContent onActivate = EditorGUIUtility.TrTextContent("(Deprecated) On Activate");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.onDeactivate"/>.</summary>
            public static readonly GUIContent onDeactivate = EditorGUIUtility.TrTextContent("(Deprecated) On Deactivate");
            /// <summary><see cref="GUIContent"/> for the header label of First/Last Hover events.</summary>
            public static readonly GUIContent firstLastHoverEventsHeader = EditorGUIUtility.TrTextContent("First/Last Hover", "Similar to Hover except called only when the first Interactor begins hovering over this Interactable as the sole hovering Interactor, or when the last remaining hovering Interactor ends hovering.");
            /// <summary><see cref="GUIContent"/> for the header label of Hover events.</summary>
            public static readonly GUIContent hoverEventsHeader = EditorGUIUtility.TrTextContent("Hover", "Called when an Interactor begins hovering over this Interactable (Entered), or ends hovering (Exited).");
            /// <summary><see cref="GUIContent"/> for the header label of First/Last Hover events.</summary>
            public static readonly GUIContent firstLastSelectEventsHeader = EditorGUIUtility.TrTextContent("First/Last Select", "Similar to Select except called only when the first Interactor begins selecting this Interactable as the sole selecting Interactor, or when the last remaining selecting Interactor ends selecting.");
            /// <summary><see cref="GUIContent"/> for the header label of First/Last Focus events.</summary>
            public static readonly GUIContent firstLastFocusEventsHeader = EditorGUIUtility.TrTextContent("First/Last Focus", "Similar to focus except called only when the first Interactor gains focus on this Interactable as the sole focusing Interactor, or when the last remaining selecting Interactor loses focus.");
            /// <summary><see cref="GUIContent"/> for the header label of Select events.</summary>
            public static readonly GUIContent selectEventsHeader = EditorGUIUtility.TrTextContent("Select", "Called when an Interactor begins selecting this Interactable (Entered), or ends selecting (Exited).");
            /// <summary><see cref="GUIContent"/> for the header label of Select events.</summary>
            public static readonly GUIContent focusEventsHeader = EditorGUIUtility.TrTextContent("Focus", "Called when an Interactor gains focus on this Interactable (Entered), or loses focus (Exited).");
            /// <summary><see cref="GUIContent"/> for the header label of Activate events.</summary>
            public static readonly GUIContent activateEventsHeader = EditorGUIUtility.TrTextContent("Activate", "Called when the Interactor that is selecting this Interactable sends a command to activate (Activated), or deactivate (Deactivated). Not to be confused with the active state of a GameObject.");

            /// <summary>The help box message when Multiple is not supported by the script and the serialized field is Multiple.</summary>
            /// <seealso cref="selectMultipleAllowed"/>
            public static readonly GUIContent multipleNotSupported = EditorGUIUtility.TrTextContent("Multiple is not supported by this component script.");

            /// <summary>The help box message when deprecated Interactable Events are being used.</summary>
            public static readonly GUIContent deprecatedEventsInUse = EditorGUIUtility.TrTextContent("Some deprecated Interactable Events are being used. These deprecated events will be removed in a future version. Please convert these to use the newer events, and update script method signatures for Dynamic listeners.");

            /// <summary>The help box message when the distance calculation is being overridden.</summary>
            public static readonly GUIContent distanceCalculationOverride = EditorGUIUtility.TrTextContent("The distance calculation is being overridden and driven by another method.");

            /// <summary>The gaze configuration foldout.</summary>
            public static readonly GUIContent gazeConfiguration = EditorGUIUtility.TrTextContent("Gaze Configuration", "Settings for gaze interactions.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.allowGazeInteraction"/>.</summary>
            public static readonly GUIContent allowGazeInteraction = EditorGUIUtility.TrTextContent("Allow Gaze Interaction", "Allows gaze Interactors to interact with this Interactable. If false, this Interactor will receive no interactable events from gaze Interactors.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.allowGazeSelect"/>.</summary>
            public static readonly GUIContent allowGazeSelect = EditorGUIUtility.TrTextContent("Allow Gaze Select", "Allows gaze Interactors to select this Interactable.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.overrideGazeTimeToSelect"/>.</summary>
            public static readonly GUIContent overrideGazeTimeToSelect = EditorGUIUtility.TrTextContent("Override Gaze Time To Select", "Enables this Interactable to override hover to select time of a gaze Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.gazeTimeToSelect"/>.</summary>
            public static readonly GUIContent gazeTimeToSelect = EditorGUIUtility.TrTextContent("Gaze Time To Select", "Number of seconds for which a gaze Interactor must hover over this Interactable to select it if Hover To Select is enabled on the Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.overrideTimeToAutoDeselectGaze"/>.</summary>
            public static readonly GUIContent overrideTimeToAutoDeselectGaze = EditorGUIUtility.TrTextContent("Override Time To Auto Deselect", "Enables this interactable to override the auto deselect time of a gaze Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.timeToAutoDeselectGaze"/>.</summary>
            public static readonly GUIContent timeToAutoDeselectGaze = EditorGUIUtility.TrTextContent("Time To Auto Deselect", "Number of seconds that the interactable will remain selected by a gaze Interactor before being automatically deselected if Auto Deselect is enabled on the Interactor .");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.allowGazeAssistance"/>.</summary>
            public static readonly GUIContent allowGazeAssistance = EditorGUIUtility.TrTextContent("Allow Gaze Assistance", "Enables gaze assistance, which allows a gaze Interactor to place a snap volume at this interactable for ray Interactors to snap to.");
            
            /// <summary>The Interactable filters foldout.</summary>
            public static readonly GUIContent interactableFilters = EditorGUIUtility.TrTextContent("Interactable Filters", "Add filters to extend this interactable without needing to create a derived behavior.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.startingSelectFilters"/>.</summary>
            public static readonly GUIContent startingHoverFilters = EditorGUIUtility.TrTextContent("Starting Hover Filters", "The hover filters that this Interactable will automatically link at startup (optional, may be empty). Used as additional hover validations for this Interactable.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.startingSelectFilters"/>.</summary>
            public static readonly GUIContent startingSelectFilters = EditorGUIUtility.TrTextContent("Starting Select Filters", "The select filters that this Interactable will automatically link at startup (optional, may be empty). Used as additional select validations for this Interactable.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRBaseInteractable.startingInteractionStrengthFilters"/>.</summary>
            public static readonly GUIContent startingInteractionStrengthFilters = EditorGUIUtility.TrTextContent("Starting Interaction Strength Filters", "The interaction strength filters that this Interactable will automatically link at startup (optional, may be empty). Used to modify the default interaction strength of an Interactor relative to this Interactable.");
            /// <summary>The list of runtime hover filters.</summary>
            public static readonly GUIContent hoverFiltersHeader = EditorGUIUtility.TrTextContent("Hover Filters", "The list is populated in Awake, during Play mode.");
            /// <summary>The list of runtime select filters.</summary>
            public static readonly GUIContent selectFiltersHeader = EditorGUIUtility.TrTextContent("Select Filters", "The list is populated in Awake, during Play mode.");
            /// <summary>The list of runtime interaction strength filters.</summary>
            public static readonly GUIContent interactionStrengthFiltersHeader = EditorGUIUtility.TrTextContent("Interaction Strength Filters", "The list is populated in Awake, during Play mode.");
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>
        protected virtual void OnEnable()
        {
            m_InteractionManager = serializedObject.FindProperty("m_InteractionManager");
            m_Colliders = serializedObject.FindProperty("m_Colliders");
            m_InteractionLayerMask = serializedObject.FindProperty("m_InteractionLayerMask");
            m_InteractionLayers = serializedObject.FindProperty("m_InteractionLayers");
            m_DistanceCalculationMode = serializedObject.FindProperty("m_DistanceCalculationMode");
            m_SelectMode = serializedObject.FindProperty("m_SelectMode");
            m_FocusMode = serializedObject.FindProperty("m_FocusMode");
            m_CustomReticle = serializedObject.FindProperty("m_CustomReticle");

            m_AllowGazeInteraction = serializedObject.FindProperty("m_AllowGazeInteraction");
            m_AllowGazeSelect = serializedObject.FindProperty("m_AllowGazeSelect");
            m_OverrideGazeTimeToSelect = serializedObject.FindProperty("m_OverrideGazeTimeToSelect");
            m_GazeTimeToSelect = serializedObject.FindProperty("m_GazeTimeToSelect");
            m_AllowGazeAssistance = serializedObject.FindProperty("m_AllowGazeAssistance");
            m_OverrideTimeToAutoDeselectGaze = serializedObject.FindProperty("m_OverrideTimeToAutoDeselectGaze");
            m_TimeToAutoDeselectGaze = serializedObject.FindProperty("m_TimeToAutoDeselectGaze");
            
            m_FirstHoverEntered = serializedObject.FindProperty("m_FirstHoverEntered");
            m_LastHoverExited = serializedObject.FindProperty("m_LastHoverExited");
            m_HoverEntered = serializedObject.FindProperty("m_HoverEntered");
            m_HoverExited = serializedObject.FindProperty("m_HoverExited");
            m_FirstSelectEntered = serializedObject.FindProperty("m_FirstSelectEntered");
            m_LastSelectExited = serializedObject.FindProperty("m_LastSelectExited");
            m_SelectEntered = serializedObject.FindProperty("m_SelectEntered");
            m_SelectExited = serializedObject.FindProperty("m_SelectExited");
            m_FirstFocusEntered = serializedObject.FindProperty("m_FirstFocusEntered");
            m_LastFocusExited = serializedObject.FindProperty("m_LastFocusExited");
            m_FocusEntered = serializedObject.FindProperty("m_FocusEntered");
            m_FocusExited = serializedObject.FindProperty("m_FocusExited");
            m_Activated = serializedObject.FindProperty("m_Activated");
            m_Deactivated = serializedObject.FindProperty("m_Deactivated");

            m_OnFirstHoverEntered = serializedObject.FindProperty("m_OnFirstHoverEntered");
            m_OnHoverEntered = serializedObject.FindProperty("m_OnHoverEntered");
            m_OnHoverExited = serializedObject.FindProperty("m_OnHoverExited");
            m_OnLastHoverExited = serializedObject.FindProperty("m_OnLastHoverExited");
            m_OnSelectEntered = serializedObject.FindProperty("m_OnSelectEntered");
            m_OnSelectExited = serializedObject.FindProperty("m_OnSelectExited");
            m_OnSelectCanceled = serializedObject.FindProperty("m_OnSelectCanceled");
            m_OnActivate = serializedObject.FindProperty("m_OnActivate");
            m_OnDeactivate = serializedObject.FindProperty("m_OnDeactivate");

            m_OnFirstHoverEnteredCalls = m_OnFirstHoverEntered.FindPropertyRelative("m_PersistentCalls.m_Calls");
            m_OnLastHoverExitedCalls = m_OnLastHoverExited.FindPropertyRelative("m_PersistentCalls.m_Calls");
            m_OnHoverEnteredCalls = m_OnHoverEntered.FindPropertyRelative("m_PersistentCalls.m_Calls");
            m_OnHoverExitedCalls = m_OnHoverExited.FindPropertyRelative("m_PersistentCalls.m_Calls");
            m_OnSelectEnteredCalls = m_OnSelectEntered.FindPropertyRelative("m_PersistentCalls.m_Calls");
            m_OnSelectExitedCalls = m_OnSelectExited.FindPropertyRelative("m_PersistentCalls.m_Calls");
            m_OnSelectCanceledCalls = m_OnSelectCanceled.FindPropertyRelative("m_PersistentCalls.m_Calls");
            m_OnActivateCalls = m_OnActivate.FindPropertyRelative("m_PersistentCalls.m_Calls");
            m_OnDeactivateCalls = m_OnDeactivate.FindPropertyRelative("m_PersistentCalls.m_Calls");

            var selectAttribute = (CanSelectMultipleAttribute)Attribute.GetCustomAttribute(target.GetType(), typeof(CanSelectMultipleAttribute));
            selectMultipleAllowed = selectAttribute?.allowMultiple ?? true;

            var focusAttribute = (CanFocusMultipleAttribute)Attribute.GetCustomAttribute(target.GetType(), typeof(CanFocusMultipleAttribute)); 
            focusMultipleAllowed = focusAttribute?.allowMultiple ?? true;

            m_GazeConfigurationExpanded = SessionState.GetBool(k_GazeConfigurationExpandedKey, false);
            m_FiltersExpanded = SessionState.GetBool(k_FiltersExpandedKey, false);

            m_StartingHoverFilters = serializedObject.FindProperty("m_StartingHoverFilters");
            m_StartingSelectFilters = serializedObject.FindProperty("m_StartingSelectFilters");
            m_StartingInteractionStrengthFilters = serializedObject.FindProperty("m_StartingInteractionStrengthFilters");

            var interactable = (XRBaseInteractable)target;
            m_HoverFilters = new ReadOnlyReorderableList<IXRHoverFilter>(new List<IXRHoverFilter>(), BaseContents.hoverFiltersHeader, k_HoverFiltersExpandedKey)
            {
                isExpanded = SessionState.GetBool(k_HoverFiltersExpandedKey, true),
                updateElements = list => interactable.hoverFilters.GetAll(list),
                onListReordered = (element, newIndex) => interactable.hoverFilters.MoveTo(element, newIndex),
            };

            m_SelectFilters = new ReadOnlyReorderableList<IXRSelectFilter>(new List<IXRSelectFilter>(), BaseContents.selectFiltersHeader, k_SelectFiltersExpandedKey)
            {
                isExpanded = SessionState.GetBool(k_SelectFiltersExpandedKey, true),
                updateElements = list => interactable.selectFilters.GetAll(list),
                onListReordered = (element, newIndex) => interactable.selectFilters.MoveTo(element, newIndex),
            };

            m_InteractionStrengthFilters = new ReadOnlyReorderableList<IXRInteractionStrengthFilter>(new List<IXRInteractionStrengthFilter>(), BaseContents.interactionStrengthFiltersHeader, k_InteractionStrengthFiltersExpandedKey)
            {
                isExpanded = SessionState.GetBool(k_InteractionStrengthFiltersExpandedKey, true),
                updateElements = list => interactable.interactionStrengthFilters.GetAll(list),
                onListReordered = (element, newIndex) => interactable.interactionStrengthFilters.MoveTo(element, newIndex),
            };
        }

        /// <inheritdoc />
        /// <seealso cref="DrawBeforeProperties"/>
        /// <seealso cref="DrawProperties"/>
        /// <seealso cref="BaseInteractionEditor.DrawDerivedProperties"/>
        /// <seealso cref="DrawFilters"/>
        /// <seealso cref="DrawEvents"/>
        protected override void DrawInspector()
        {
            DrawBeforeProperties();
            DrawProperties();
            DrawDerivedProperties();

            EditorGUILayout.Space();

            DrawFilters();

            EditorGUILayout.Space();

            DrawEvents();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the section of the custom inspector before <see cref="DrawProperties"/>.
        /// By default, this draws the read-only Script property.
        /// </summary>
        protected virtual void DrawBeforeProperties()
        {
            DrawScript();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the property fields. Override this method to customize the
        /// properties shown in the Inspector. This is typically the method overridden
        /// when a derived behavior adds additional serialized properties
        /// that should be displayed in the Inspector.
        /// </summary>
        protected virtual void DrawProperties()
        {
            DrawCoreConfiguration();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the event properties. Override this method to customize the
        /// events shown in the Inspector. This is typically the method overridden
        /// when a derived behavior adds additional serialized event properties
        /// that should be displayed in the Inspector.
        /// </summary>
        protected virtual void DrawEvents()
        {
            DrawInteractableEvents();
        }

        /// <summary>
        /// Draw the core group of property fields. These are the main properties
        /// that appear before any other spaced section in the inspector.
        /// </summary>
        protected virtual void DrawCoreConfiguration()
        {
            DrawInteractionManagement();
            DrawDistanceCalculationMode();
            EditorGUILayout.PropertyField(m_CustomReticle, BaseContents.customReticle);
            DrawSelectionConfiguration();
            DrawFocusConfiguration();
            DrawGazeConfiguration();
        }

        /// <summary>
        /// Draw the property fields related to gaze configuration.
        /// </summary>
        protected virtual void DrawGazeConfiguration()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_GazeConfigurationExpanded = EditorGUILayout.Foldout(m_GazeConfigurationExpanded, BaseContents.gazeConfiguration, true);
                if (check.changed)
                    SessionState.SetBool(k_GazeConfigurationExpandedKey, m_GazeConfigurationExpanded);
            }

            if (!m_GazeConfigurationExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_AllowGazeInteraction, BaseContents.allowGazeInteraction);
                if (m_AllowGazeInteraction.boolValue)
                {
                    EditorGUILayout.PropertyField(m_AllowGazeSelect, BaseContents.allowGazeSelect);
                    if (m_AllowGazeSelect.boolValue)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.PropertyField(m_OverrideGazeTimeToSelect, BaseContents.overrideGazeTimeToSelect);
                            if (m_OverrideGazeTimeToSelect.boolValue)
                            {
                                using (new EditorGUI.IndentLevelScope())
                                {
                                    EditorGUILayout.PropertyField(m_GazeTimeToSelect, BaseContents.gazeTimeToSelect);
                                }
                            }
                            EditorGUILayout.PropertyField(m_OverrideTimeToAutoDeselectGaze, BaseContents.overrideTimeToAutoDeselectGaze);
                            if (m_OverrideTimeToAutoDeselectGaze.boolValue)
                            {
                                using (new EditorGUI.IndentLevelScope())
                                {
                                    EditorGUILayout.PropertyField(m_TimeToAutoDeselectGaze, BaseContents.timeToAutoDeselectGaze);
                                }
                            }
                        }
                    }
                        
                    EditorGUILayout.PropertyField(m_AllowGazeAssistance, BaseContents.allowGazeAssistance);
                }
            }
        }
        
        /// <summary>
        /// Draw the property fields related to selection configuration.
        /// </summary>
        protected virtual void DrawSelectionConfiguration()
        {
            if (m_SelectMode.intValue == (int)InteractableSelectMode.Multiple && !selectMultipleAllowed)
                EditorGUILayout.HelpBox(BaseContents.multipleNotSupported.text, MessageType.Error);

            XRInteractionEditorGUI.EnumPropertyField<InteractableSelectMode>(m_SelectMode, BaseContents.selectMode, IsSelectModeOptionEnabled);
        }

        /// <summary>
        /// Draw the property fields related to focus configuration.
        /// </summary>
        protected virtual void DrawFocusConfiguration()
        {
            if (m_FocusMode.intValue == (int)InteractableFocusMode.Multiple && !focusMultipleAllowed)
                EditorGUILayout.HelpBox(BaseContents.multipleNotSupported.text, MessageType.Error);

            XRInteractionEditorGUI.EnumPropertyField<InteractableFocusMode>(m_FocusMode, BaseContents.focusMode, IsFocusModeOptionEnabled);
        }

        /// <summary>
        /// Draw the Interactable Filter foldout.
        /// </summary>
        protected virtual void DrawFilters()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_FiltersExpanded = EditorGUILayout.Foldout(m_FiltersExpanded, BaseContents.interactableFilters, true);
                if (check.changed)
                    SessionState.SetBool(k_FiltersExpandedKey, m_FiltersExpanded);
            }

            if (!m_FiltersExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                DrawFiltersNested();
            }
        }

        /// <summary>
        /// Draw the property fields related to the hover and select filters.
        /// </summary>
        protected virtual void DrawFiltersNested()
        {
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.PropertyField(m_StartingHoverFilters, BaseContents.startingHoverFilters);
                EditorGUILayout.PropertyField(m_StartingSelectFilters, BaseContents.startingSelectFilters);
                EditorGUILayout.PropertyField(m_StartingInteractionStrengthFilters, BaseContents.startingInteractionStrengthFilters);
            }

            if (!Application.isPlaying)
                return;

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox("Filters cannot be multi-edited.", MessageType.None);
                return;
            }

            m_HoverFilters.DoLayoutList();
            m_SelectFilters.DoLayoutList();
            m_InteractionStrengthFilters.DoLayoutList();
        }

        bool IsSelectModeOptionEnabled(Enum arg) => (InteractableSelectMode)arg != InteractableSelectMode.Multiple || selectMultipleAllowed;
        bool IsFocusModeOptionEnabled(Enum arg) => (InteractableFocusMode)arg != InteractableFocusMode.Multiple || focusMultipleAllowed;

        /// <summary>
        /// Draw the property fields related to interaction management.
        /// </summary>
        protected virtual void DrawInteractionManagement()
        {
            EditorGUILayout.PropertyField(m_InteractionManager, BaseContents.interactionManager);
            EditorGUILayout.PropertyField(m_InteractionLayers, BaseContents.interactionLayers);
            if (XRInteractionEditorSettings.Instance.showOldInteractionLayerMaskInInspector)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_InteractionLayerMask, BaseContents.interactionLayerMask);
                }
            }

            EditorGUILayout.PropertyField(m_Colliders, BaseContents.colliders, true);
        }

        /// <summary>
        /// Draw the Distance Calculation Mode property and the distance calculation override message.
        /// </summary>
        protected virtual void DrawDistanceCalculationMode()
        {
            EditorGUILayout.PropertyField(m_DistanceCalculationMode, BaseContents.distanceCalculationMode);
            if (Application.isPlaying
                && !m_DistanceCalculationMode.hasMultipleDifferentValues
                && target is XRBaseInteractable interactable && interactable != null
                && interactable.getDistanceOverride != null)
            {
                EditorGUILayout.HelpBox(BaseContents.distanceCalculationOverride.text, MessageType.None);
            }
        }

        /// <summary>
        /// Draw the Interactable Events foldout.
        /// </summary>
        /// <seealso cref="DrawInteractableEventsNested"/>
        protected virtual void DrawInteractableEvents()
        {
#pragma warning disable 618 // One-time migration of deprecated events.
            if (IsDeprecatedEventsInUse())
            {
                EditorGUILayout.HelpBox(BaseContents.deprecatedEventsInUse.text, MessageType.Warning);
                if (GUILayout.Button("Migrate Events"))
                {
                    if (m_OnSelectCanceledCalls.arraySize > 0 || m_OnSelectCanceledCalls.hasMultipleDifferentValues)
                        Debug.LogWarning("Unable to migrate the deprecated On Select Canceled event since there" +
                            " is no corresponding event as Select Exited will fire in both cases.", target);

                    serializedObject.ApplyModifiedProperties();
                    MigrateEvents(targets);
                    serializedObject.SetIsDifferentCacheDirty();
                    serializedObject.Update();
                }
            }
#pragma warning restore 618

            m_FirstHoverEntered.isExpanded = EditorGUILayout.Foldout(m_FirstHoverEntered.isExpanded, EditorGUIUtility.TrTempContent("Interactable Events"), true);
            if (m_FirstHoverEntered.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawInteractableEventsNested();
                }
            }
        }

        /// <summary>
        /// Draw the nested contents of the Interactable Events foldout.
        /// </summary>
        /// <seealso cref="DrawInteractableEvents"/>
        protected virtual void DrawInteractableEventsNested()
        {
            EditorGUILayout.LabelField(BaseContents.firstLastHoverEventsHeader, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_FirstHoverEntered);
            if (m_OnFirstHoverEnteredCalls.arraySize > 0 || m_OnFirstHoverEnteredCalls.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(m_OnFirstHoverEntered, BaseContents.onFirstHoverEntered);
            EditorGUILayout.PropertyField(m_LastHoverExited);
            if (m_OnLastHoverExitedCalls.arraySize > 0 || m_OnLastHoverExitedCalls.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(m_OnLastHoverExited, BaseContents.onLastHoverExited);

            EditorGUILayout.LabelField(BaseContents.hoverEventsHeader, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_HoverEntered);
            if (m_OnHoverEnteredCalls.arraySize > 0 || m_OnHoverEnteredCalls.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(m_OnHoverEntered, BaseContents.onHoverEntered);
            EditorGUILayout.PropertyField(m_HoverExited);
            if (m_OnHoverExitedCalls.arraySize > 0 || m_OnHoverExitedCalls.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(m_OnHoverExited, BaseContents.onHoverExited);

            EditorGUILayout.LabelField(BaseContents.firstLastSelectEventsHeader, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_FirstSelectEntered);
            EditorGUILayout.PropertyField(m_LastSelectExited);

            EditorGUILayout.LabelField(BaseContents.selectEventsHeader, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_SelectEntered);
            if (m_OnSelectEnteredCalls.arraySize > 0 || m_OnSelectEnteredCalls.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(m_OnSelectEntered, BaseContents.onSelectEntered);
            EditorGUILayout.PropertyField(m_SelectExited);
            if (m_OnSelectExitedCalls.arraySize > 0 || m_OnSelectExitedCalls.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(m_OnSelectExited, BaseContents.onSelectExited);
            if (m_OnSelectCanceledCalls.arraySize > 0 || m_OnSelectCanceledCalls.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(m_OnSelectCanceled, BaseContents.onSelectCanceled);

            EditorGUILayout.LabelField(BaseContents.firstLastFocusEventsHeader, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_FirstFocusEntered);
            EditorGUILayout.PropertyField(m_LastFocusExited);

            EditorGUILayout.LabelField(BaseContents.focusEventsHeader, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_FocusEntered);
            EditorGUILayout.PropertyField(m_FocusExited);

            EditorGUILayout.LabelField(BaseContents.activateEventsHeader, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_Activated);
            if (m_OnActivateCalls.arraySize > 0 || m_OnActivateCalls.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(m_OnActivate, BaseContents.onActivate);
            EditorGUILayout.PropertyField(m_Deactivated);
            if (m_OnDeactivateCalls.arraySize > 0 || m_OnDeactivateCalls.hasMultipleDifferentValues)
                EditorGUILayout.PropertyField(m_OnDeactivate, BaseContents.onDeactivate);
        }
    }
}