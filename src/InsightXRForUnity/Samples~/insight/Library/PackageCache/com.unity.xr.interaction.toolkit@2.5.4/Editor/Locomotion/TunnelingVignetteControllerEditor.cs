using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="TunnelingVignetteController"/>.
    /// </summary>
    [CustomEditor(typeof(TunnelingVignetteController), true)]
    public class TunnelingVignetteControllerEditor : BaseInteractionEditor
    {
        const float k_Padding = 2f;
        const int k_DragHandleWidth = ReorderableList.Defaults.dragHandleWidth;

        static float indent => EditorGUI.indentLevel * 15f;

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="TunnelingVignetteController.defaultParameters"/>.</summary>
        protected SerializedProperty m_DefaultParameters;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="TunnelingVignetteController.currentParameters"/>.</summary>
        protected SerializedProperty m_CurrentParameters;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="TunnelingVignetteController.locomotionVignetteProviders"/>.</summary>
        protected SerializedProperty m_LocomotionVignetteProviders;

        /// <summary>
        /// List used to customize the default layout of <see cref="TunnelingVignetteController.locomotionVignetteProviders"/>.
        /// </summary>
        ReorderableList m_EditorProviderList;

        /// <summary>
        /// List for storing <see cref="LocomotionProvider"/> names to be displayed in the preview drop down menu.
        /// </summary>
        readonly List<string> m_PreviewList = new List<string>();

        int m_PreviewIndex;

        static bool s_HeightsMeasured;
        static float s_UnexpandedHeight, s_ExpandedHeight;

        /// <summary>
        /// Array for storing all of the <see cref="LocomotionProvider"/> components in the scene.
        /// </summary>
        static LocomotionProvider[] s_CachedLocomotionProviders;

        /// <summary>
        /// Dictionary for storing <see cref="LocomotionProvider"/> components by the GameObjects they are attached to in the scene.
        /// </summary>
        readonly Dictionary<string, List<LocomotionProvider>> m_LocomotionProvidersByGOName = new Dictionary<string, List<LocomotionProvider>>();

        /// <summary>
        /// Dictionary for storing names of <see cref="LocomotionProvider"/> components by the GameObjects they are attached to in the scene.
        /// </summary>
        readonly Dictionary<string, List<string>> m_LocomotionProviderNamesByGOName = new Dictionary<string, List<string>>();

        /// <summary>
        /// List for storing the selected index of the drop down menu. The selected index is used for specifying which <see cref="LocomotionProvider"/> component
        /// to select for the Locomotion Provider field of the Inspector if multiple <see cref="LocomotionProvider"/> components are present on the selected Object.
        /// </summary>
        readonly List<int> m_LocomotionProviderSelectedIndexes = new List<int>();

        /// <summary>
        /// The <see cref="TunnelingVignetteController"/> component targeted in the Inspector.
        /// </summary>
        TunnelingVignetteController m_TunnelingVignetteController;

        /// <summary>
        /// List that represents the <see cref="TunnelingVignetteController.locomotionVignetteProviders"/> of this target.
        /// </summary>
        List<LocomotionVignetteProvider> m_ControllerLocomotionVignetteProviders = new List<LocomotionVignetteProvider>();

        /// <summary>
        /// Dictionary for preserving the selected preview of each <see cref="TunnelingVignetteController"/> target between each time this Editor script is enabled (shown in the Inspector).
        /// </summary>
        static readonly Dictionary<TunnelingVignetteController, int> s_ControllerPreviewIndex = new Dictionary<TunnelingVignetteController, int>();

        VignetteParameterProperties m_DefaultParameterProperties;
        VignetteParameterProperties m_CurrentParameterProperties;

        /// <summary>
        /// Represents a struct of <see cref="SerializedProperty"/> for the members of the class <see cref="VignetteParameters"/>.
        /// </summary>
        protected readonly struct VignetteParameterProperties
        {
            public readonly SerializedProperty apertureSize;
            public readonly SerializedProperty featheringEffect;
            public readonly SerializedProperty easeInTime;
            public readonly SerializedProperty easeOutTime;
            public readonly SerializedProperty easeInTimeLock;
            public readonly SerializedProperty easeOutDelayTime;
            public readonly SerializedProperty vignetteColor;
            public readonly SerializedProperty vignetteColorBlend;
            public readonly SerializedProperty apertureVerticalPosition;

            public VignetteParameterProperties(SerializedProperty parameterProperties)
            {
                apertureSize = parameterProperties.FindPropertyRelative("m_ApertureSize");
                featheringEffect = parameterProperties.FindPropertyRelative("m_FeatheringEffect");
                easeInTime = parameterProperties.FindPropertyRelative("m_EaseInTime");
                easeOutTime = parameterProperties.FindPropertyRelative("m_EaseOutTime");
                easeInTimeLock = parameterProperties.FindPropertyRelative("m_EaseInTimeLock");
                easeOutDelayTime = parameterProperties.FindPropertyRelative("m_EaseOutDelayTime");
                vignetteColor = parameterProperties.FindPropertyRelative("m_VignetteColor");
                vignetteColorBlend = parameterProperties.FindPropertyRelative("m_VignetteColorBlend");
                apertureVerticalPosition = parameterProperties.FindPropertyRelative("m_ApertureVerticalPosition");
            }

            public void SetDefaultValues()
            {
                apertureSize.floatValue = VignetteParameters.Defaults.apertureSizeDefault;
                featheringEffect.floatValue = VignetteParameters.Defaults.featheringEffectDefault;
                easeInTime.floatValue = VignetteParameters.Defaults.easeInTimeDefault;
                easeOutTime.floatValue = VignetteParameters.Defaults.easeOutTimeDefault;
                easeInTimeLock.boolValue = VignetteParameters.Defaults.easeInTimeLockDefault;
                easeOutDelayTime.floatValue = VignetteParameters.Defaults.easeOutDelayTimeDefault;
                vignetteColor.colorValue = VignetteParameters.Defaults.vignetteColorDefault;
                vignetteColorBlend.colorValue = VignetteParameters.Defaults.vignetteColorBlendDefault;
                apertureVerticalPosition.floatValue = VignetteParameters.Defaults.apertureVerticalPositionDefault;
            }
        }

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for the preview in editor drop-down menu.</summary>
            public static readonly GUIContent previewInEditor = EditorGUIUtility.TrTextContent("Preview In Editor", "(Editor Only) Select which set of vignette parameters to preview. " +
                "Adding or removing settings in the Locomotion Vignette Providers list will automatically update this preview menu.");
            /// <summary><see cref="GUIContent"/> for when the preview in editor is disabled in Play mode.</summary>
            public static readonly GUIContent previewInEditorDisabled = EditorGUIUtility.TrTextContent("Preview In Editor (Disabled in Play mode)" );
            /// <summary><see cref="GUIContent"/> for <see cref="TunnelingVignetteController.defaultParameters"/>.</summary>
            public static readonly GUIContent defaultParameters = EditorGUIUtility.TrTextContent("Default Parameters", "The default parameters of this component to drive the tunneling vignette material. " +
                "These values can be override to create customized tunneling vignette effects.");
            /// <summary><see cref="GUIContent"/> for <see cref="TunnelingVignetteController.currentParameters"/>.</summary>
            public static readonly GUIContent currentParameters = EditorGUIUtility.TrTextContent("Current Parameters", "View the current parameters that are controlling the vignette material. " +
                "These parameter values will be dynamically updated when Locomotion Vignette Providers are triggering the easing in and easing out transitions");
            /// <summary><see cref="GUIContent"/> for <see cref="TunnelingVignetteController.locomotionVignetteProviders"/>.</summary>
            public static readonly GUIContent locomotionVignetteProviders = EditorGUIUtility.TrTextContent("Locomotion Vignette Providers", "The list of Locomotion Providers and settings used for triggering the tunneling vignette.");
            /// <summary><see cref="GUIContent"/> for <see cref="LocomotionVignetteProvider.locomotionProvider"/>.</summary>
            public static readonly GUIContent locomotionProvider = EditorGUIUtility.TrTextContent("Locomotion Provider", "Select a Locomotion Provider component to trigger the tunneling vignette effects.");
            /// <summary><see cref="GUIContent"/> for <see cref="LocomotionVignetteProvider.enabled"/>.</summary>
            public static readonly GUIContent enabled = EditorGUIUtility.TrTextContent("Enabled", "A shortcut to enable or disable the selected Locomotion Provider to trigger its configured tunneling vignette effects.");
            /// <summary><see cref="GUIContent"/> for <see cref="LocomotionVignetteProvider.overrideDefaultParameters"/>.</summary>
            public static readonly GUIContent overrideDefaultParameters = EditorGUIUtility.TrTextContent("Override Default Parameters", "If enabled, the Locomotion Provider will override the Default Parameters of this component.");
            /// <summary><see cref="GUIContent"/> for <see cref="VignetteParameters.apertureSize"/>.</summary>
            public static readonly GUIContent apertureSize = EditorGUIUtility.TrTextContent("Aperture Size", "The diameter of the inner transparent circle of the tunneled vignette. " +
                "A vignette provider with a smaller aperture size has a higher priority in controlling the material.");
            /// <summary><see cref="GUIContent"/> for <see cref="VignetteParameters.featheringEffect"/>.</summary>
            public static readonly GUIContent featheringEffect = EditorGUIUtility.TrTextContent("Feathering Effect", "The degree of smoothly blending the edges between the aperture and full visual cut-off to " +
                "add a gradual transition from the transparent aperture to the vignette edges.");
            /// <summary><see cref="GUIContent"/> for <see cref="VignetteParameters.easeInTime"/>.</summary>
            public static readonly GUIContent easeInTime = EditorGUIUtility.TrTextContent("Ease In Time", "The transition time (in seconds) of easing in the tunneling vignette. " +
                "Set this value to reduce the potential distraction from instantaneously changing the user's field of view.");
            /// <summary><see cref="GUIContent"/> for <see cref="VignetteParameters.easeOutTime"/>.</summary>
            public static readonly GUIContent easeOutTime = EditorGUIUtility.TrTextContent("Ease Out Time", "The transition time (in seconds) of easing out the tunneling vignette. " +
                "Set this value to reduce the potential distraction from instantaneously changing the user's field of view.");
            /// <summary><see cref="GUIContent"/> for <see cref="VignetteParameters.easeInTimeLock"/>.</summary>
            public static readonly GUIContent easeInTimeLock = EditorGUIUtility.TrTextContent("Ease In Time Lock", "Enable this option if you want to have the easing-in transition persist until it is complete. " +
                "This can be useful for instant changes, such as snap turn and teleportation.");
            /// <summary><see cref="GUIContent"/> for <see cref="VignetteParameters.easeOutDelayTime"/>.</summary>
            public static readonly GUIContent easeOutDelayTime = EditorGUIUtility.TrTextContent("Ease Out Delay Time", "The delay time (in seconds) before starting to ease out of the tunneling vignette.");
           /// <summary><see cref="GUIContent"/> for <see cref="VignetteParameters.vignetteColor"/>.</summary>
            public static readonly GUIContent vignetteColor = EditorGUIUtility.TrTextContent("Vignette Color", "The primary color of the visual cut-off area of the vignette.");
            /// <summary><see cref="GUIContent"/> for <see cref="VignetteParameters.vignetteColorBlend"/>.</summary>
            public static readonly GUIContent vignetteColorBlend = EditorGUIUtility.TrTextContent("Vignette Color Blend", "The optional color to add color blending to the vignette. " +
                "Use this optionally to create a color gradient for the vignette.");
            /// <summary><see cref="GUIContent"/> for <see cref="VignetteParameters.apertureVerticalPosition"/>.</summary>
            public static readonly GUIContent apertureVerticalPosition = EditorGUIUtility.TrTextContent("Aperture Vertical Position", "The vertical position offset of the vignette. " +
                "Change this value will change the local y-position of the GameObject that this script is attached to.");
            /// <summary><see cref="GUIContent"/> for the "Reset to Defaults" button for presetting the component's parameters.</summary>
            public static readonly GUIContent resetToDefaults = EditorGUIUtility.TrTextContent("Reset to Defaults", "Set the Default Parameters to default values.");
        }

        /// <summary>
        /// Unity calls this function automatically when an Inspector shows a <see cref="TunnelingVignetteController"/> component.
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>
        protected virtual void OnEnable()
        {
            m_DefaultParameters = serializedObject.FindProperty("m_DefaultParameters");
            m_DefaultParameterProperties = new VignetteParameterProperties(m_DefaultParameters);

            m_CurrentParameters = serializedObject.FindProperty("m_CurrentParameters");
            m_CurrentParameterProperties = new VignetteParameterProperties(m_CurrentParameters);

            m_LocomotionVignetteProviders = serializedObject.FindProperty("m_LocomotionVignetteProviders");

            m_EditorProviderList = new ReorderableList(serializedObject, m_LocomotionVignetteProviders, true, false, true, true)
            {
                drawElementCallback = DrawListElements,
                elementHeightCallback = GetElementHeight,
                onAddCallback = OnAddElement,
                onRemoveCallback = OnRemoveElement,
                onReorderCallbackWithDetails = OnReorderElementWithDetails,
            };

            m_TunnelingVignetteController = (TunnelingVignetteController)target;
            m_ControllerLocomotionVignetteProviders = m_TunnelingVignetteController.locomotionVignetteProviders;

            // Cache all the LocomotionProviders that exist in the scene.
#if UNITY_2023_1_OR_NEWER
            s_CachedLocomotionProviders = FindObjectsByType<LocomotionProvider>(FindObjectsSortMode.None);
#else
            s_CachedLocomotionProviders = FindObjectsOfType<LocomotionProvider>();
#endif
            InitializeLocomotionProviderDropDownDisplay();

            // Update the Locomotion Provider names in the preview drop down menu.
            UpdatePreviewMenu();

            // Restore the preview.
            if (s_ControllerPreviewIndex.TryGetValue(m_TunnelingVignetteController, out var previewIndex))
                m_PreviewIndex = previewIndex;
            else
                s_ControllerPreviewIndex.Add(m_TunnelingVignetteController, m_PreviewIndex);
        }

        /// <summary>
        /// Unity calls this function automatically when an Inspector no longer shows a <see cref="TunnelingVignetteController"/> component.
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>
        protected virtual void OnDisable()
        {
            // Save the preview.
            if (s_ControllerPreviewIndex.ContainsKey(m_TunnelingVignetteController))
                s_ControllerPreviewIndex[m_TunnelingVignetteController] = m_PreviewIndex;
            else
                s_ControllerPreviewIndex.Add(m_TunnelingVignetteController, m_PreviewIndex);
        }

        void OnAddElement(ReorderableList list)
        {
            m_LocomotionVignetteProviders.arraySize++;

            var index = m_LocomotionVignetteProviders.arraySize - 1;
            var element = m_LocomotionVignetteProviders.GetArrayElementAtIndex(index);

            SetElementDefaultValues(element);

            m_LocomotionProviderSelectedIndexes.Add(0);
        }

        static void SetElementDefaultValues(SerializedProperty element)
        {
            // Override the element's system default values when adding a new element to the ReorderableList.
            var locomotionProviderProp = element.FindPropertyRelative("m_LocomotionProvider");
            var enabled = element.FindPropertyRelative("m_Enabled");
            var useOverrideParameters = element.FindPropertyRelative("m_OverrideDefaultParameters");
            var parametersProperties = element.FindPropertyRelative("m_OverrideParameters");
            var parameters = new VignetteParameterProperties(parametersProperties);

            locomotionProviderProp.objectReferenceValue = null;
            enabled.boolValue = true;
            useOverrideParameters.boolValue = false;
            parameters.SetDefaultValues();
        }

        void OnReorderElementWithDetails(ReorderableList list, int oldIndex, int newIndex)
        {
            // Update the selected index of the dropdown LocomotionProvider selector.
            var oldSelectedIndex = m_LocomotionProviderSelectedIndexes[oldIndex];
            m_LocomotionProviderSelectedIndexes.RemoveAt(oldIndex);
            m_LocomotionProviderSelectedIndexes.Insert(newIndex, oldSelectedIndex);

            // Update the preview index of the preview drop down.
            if (m_PreviewIndex > 1)
            {
                // Update the preview list.
                m_PreviewList.Clear();
                m_PreviewList.Add("No Effect");
                m_PreviewList.Add("Default Parameters");
                var numFixedItems = m_PreviewList.Count;

                for (var i = 0; i < m_ControllerLocomotionVignetteProviders.Count; i++)
                {
                    m_PreviewList.Add(m_ControllerLocomotionVignetteProviders[i].locomotionProvider != null ?
                        m_ControllerLocomotionVignetteProviders[i].locomotionProvider.GetType().Name : "Locomotion Provider " + (i + 1) + " (Null)");
                }

                if (m_PreviewIndex == oldIndex + numFixedItems)
                    m_PreviewIndex = newIndex + numFixedItems;
                else if (m_PreviewIndex == newIndex + numFixedItems)
                    m_PreviewIndex = oldIndex + numFixedItems;
            }
        }

        void OnRemoveElement(ReorderableList list)
        {
            var index = m_LocomotionVignetteProviders.arraySize - 1;

            // Handles the Preview dropdown index.
            if (m_PreviewIndex - 2 == index)
                m_PreviewIndex = 0;

            m_LocomotionVignetteProviders.arraySize--;

            m_LocomotionProviderSelectedIndexes.RemoveAt(index);

            m_PreviewIndex = EditorGUILayout.Popup(Contents.previewInEditor,
                (m_ControllerLocomotionVignetteProviders.Count != 0) ? m_PreviewIndex : 0, m_PreviewList.ToArray());
        }

        void DrawListElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Get the SerializedProperties for laying them out on the inspector.
            var element = m_LocomotionVignetteProviders.GetArrayElementAtIndex(index);
            var locomotionProviderProp = element.FindPropertyRelative("m_LocomotionProvider");
            var enabled = element.FindPropertyRelative("m_Enabled");
            var useOverrideParameters = element.FindPropertyRelative("m_OverrideDefaultParameters");
            var parameterProperties = element.FindPropertyRelative("m_OverrideParameters");
            var parameters = new VignetteParameterProperties(parameterProperties);
            var locomotionProviderObj = locomotionProviderProp.objectReferenceValue;

            var x = rect.x;
            var y = rect.y + k_Padding;
            var width = rect.width - EditorGUIUtility.labelWidth + k_DragHandleWidth;
            var height = EditorGUI.GetPropertyHeight(locomotionProviderProp);

            EditorGUILayout.BeginHorizontal();

            // Layout "Locomotion Provider"
            EditorGUI.LabelField(new Rect(x, y - k_Padding, EditorGUIUtility.labelWidth - k_DragHandleWidth, height),
                Contents.locomotionProvider, locomotionProviderProp.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label);

            var objectFieldX = x + EditorGUIUtility.labelWidth - k_DragHandleWidth - indent;
            var objectFieldWidth = width + indent;
            var objectFieldRect = new Rect(objectFieldX, y, objectFieldWidth, height);

            // Show the dropdown menu for the LocomotionProvider field if its objectReferenceValue's gameObject
            // has multiple LocomotionProvider components.
            if (locomotionProviderObj != null
                && m_LocomotionProviderNamesByGOName.TryGetValue(locomotionProviderObj.name, out var locomotionProviderNames)
                && locomotionProviderNames.Count > 1
                && m_LocomotionProvidersByGOName.TryGetValue(locomotionProviderObj.name, out var locomotionProviders)
                && locomotionProviders.Count > 1)
            {
                objectFieldWidth = width + indent - EditorGUIUtility.fieldWidth - k_Padding;
                objectFieldRect.width = objectFieldWidth;

                EditorGUI.ObjectField(objectFieldRect, locomotionProviderProp, GUIContent.none);

                var dropDownRect = new Rect(objectFieldRect.xMax + k_Padding, y, EditorGUIUtility.fieldWidth, height);

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    m_LocomotionProviderSelectedIndexes[index] = EditorGUI.Popup(dropDownRect, m_LocomotionProviderSelectedIndexes[index],
                        locomotionProviderNames.ToArray());

                    if (check.changed)
                    {
                        // Change the Object by the new selected option.
                        locomotionProviderProp.objectReferenceValue = locomotionProviders[m_LocomotionProviderSelectedIndexes[index]];
                    }
                    else
                    {
                        // Make sure the displayed option matches the selected Object.
                        var locomotionProviderObjFullName = locomotionProviderObj.GetType().FullName;
                        for (var j = 0; j < locomotionProviderNames.Count; j++)
                        {
                            if (locomotionProviderObjFullName == locomotionProviders[j].GetType().FullName)
                            {
                                m_LocomotionProviderSelectedIndexes[index] = j;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                EditorGUI.ObjectField(objectFieldRect, locomotionProviderProp, GUIContent.none);
            }

            EditorGUILayout.EndHorizontal();

            // Layout "Enable Provider"
            y += height + k_Padding;
            height = EditorGUI.GetPropertyHeight(enabled);
            LayoutElement(new Rect(x, y, width, height), enabled, Contents.enabled);

            // Layout "Override Default Parameters"
            y += height + k_Padding;
            height = EditorGUI.GetPropertyHeight(useOverrideParameters);
            LayoutElement(new Rect(x, y, width, height), useOverrideParameters, Contents.overrideDefaultParameters);

            // Layout expanded fields if "Override Default Parameters" is enabled.
            if (useOverrideParameters.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    y += height + k_Padding;
                    height = EditorGUI.GetPropertyHeight(parameters.apertureSize);
                    LayoutElement(new Rect(x, y, width, height), parameters.apertureSize, Contents.apertureSize);

                    y += height + k_Padding;
                    height = EditorGUI.GetPropertyHeight(parameters.featheringEffect);
                    LayoutElement(new Rect(x, y, width, height), parameters.featheringEffect, Contents.featheringEffect);

                    y += height + k_Padding;
                    height = EditorGUI.GetPropertyHeight(parameters.easeInTime);
                    LayoutElement(new Rect(x, y, width, height), parameters.easeInTime, Contents.easeInTime);

                    y += height + k_Padding;
                    height = EditorGUI.GetPropertyHeight(parameters.easeOutTime);
                    LayoutElement(new Rect(x, y, width, height), parameters.easeOutTime, Contents.easeOutTime);

                    y += height + k_Padding;
                    height = EditorGUI.GetPropertyHeight(parameters.easeInTimeLock);
                    LayoutElement(new Rect(x, y, width, height), parameters.easeInTimeLock, Contents.easeInTimeLock);

                    y += height + k_Padding;
                    height = EditorGUI.GetPropertyHeight(parameters.easeOutDelayTime);
                    LayoutElement(new Rect(x, y, width, height), parameters.easeOutDelayTime, Contents.easeOutDelayTime);

                    y += height + k_Padding;
                    height = EditorGUI.GetPropertyHeight(parameters.vignetteColor);
                    LayoutElement(new Rect(x, y, width, height), parameters.vignetteColor, Contents.vignetteColor);

                    y += height + k_Padding;
                    height = EditorGUI.GetPropertyHeight(parameters.vignetteColorBlend);
                    LayoutElement(new Rect(x, y, width, height), parameters.vignetteColorBlend, Contents.vignetteColorBlend);

                    y += height + k_Padding;
                    height = EditorGUI.GetPropertyHeight(parameters.apertureVerticalPosition);
                    LayoutElement(new Rect(x, y, width, height), parameters.apertureVerticalPosition, Contents.apertureVerticalPosition);
                }
            }
        }

        static void LayoutElement(Rect rect, SerializedProperty serializedProperty, GUIContent labelContent)
        {
            EditorGUILayout.BeginHorizontal();

            // Set the width of the LabelField manually so that its text will not overflow horizontally to appear under the property field.
            // Manually set the bold and focused states of the label.
            var labelRect = new Rect(rect.x, rect.y - k_Padding, EditorGUIUtility.labelWidth - k_DragHandleWidth, rect.height);
            var hasPrefabOverride = serializedProperty.prefabOverride;
            var style = hasPrefabOverride ? EditorStyles.boldLabel : EditorStyles.label;
            EditorGUI.LabelField(labelRect, labelContent, style);

            var fieldRect = new Rect(rect.x + EditorGUIUtility.labelWidth - k_DragHandleWidth - indent, rect.y, rect.width + indent, rect.height);
            EditorGUI.PropertyField(fieldRect, serializedProperty, GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }

        void UpdatePreviewMenu()
        {
            m_PreviewList.Clear();
            m_PreviewList.Add("No Effect");
            m_PreviewList.Add("Default Parameters");

            for (var i = 0; i < m_ControllerLocomotionVignetteProviders.Count; i++)
            {
                var locomotionProvider = m_ControllerLocomotionVignetteProviders[i].locomotionProvider;
                m_PreviewList.Add(locomotionProvider != null ? locomotionProvider.GetType().Name : "Locomotion Provider " + (i + 1) + " (Null)");
            }
        }

        void InitializeLocomotionProviderDropDownDisplay()
        {
            if (m_LocomotionVignetteProviders == null)
                return;

            // Update to match sizes.
            while (m_LocomotionProviderSelectedIndexes.Count < m_LocomotionVignetteProviders.arraySize)
                m_LocomotionProviderSelectedIndexes.Add(0);

            while (m_LocomotionProviderSelectedIndexes.Count > m_LocomotionVignetteProviders.arraySize)
                m_LocomotionProviderSelectedIndexes.RemoveAt(m_LocomotionProviderSelectedIndexes.Count - 1);

            if (s_CachedLocomotionProviders == null || s_CachedLocomotionProviders.Length == 0)
                return;

            // Initialize Dictionaries that store LocomotionProvider names and components based on names of GameObjects
            // that have LocomotionProvider components in the scene.
            foreach (var component in s_CachedLocomotionProviders)
            {
                var go = component.gameObject;
                var goName = go.name;
                var newLocomotionProviderNames = new List<string>();

                var locomotionProvidersArray = go.GetComponents<LocomotionProvider>();
                var newLocomotionProviders = new List<LocomotionProvider>();

                foreach (var locomotionProvider in locomotionProvidersArray)
                {
                    newLocomotionProviderNames.Add(locomotionProvider.GetType().Name);
                    newLocomotionProviders.Add(locomotionProvider);
                }

                if (!m_LocomotionProviderNamesByGOName.ContainsKey(goName))
                {
                    m_LocomotionProviderNamesByGOName.Add(goName, newLocomotionProviderNames);
                }
                else
                {
                    m_LocomotionProviderNamesByGOName[goName].Clear();
                    m_LocomotionProviderNamesByGOName[goName].AddRange(newLocomotionProviderNames);
                }

                if (!m_LocomotionProvidersByGOName.ContainsKey(goName))
                {
                    m_LocomotionProvidersByGOName.Add(goName, newLocomotionProviders);
                }
                else
                {
                    m_LocomotionProvidersByGOName[goName].Clear();
                    m_LocomotionProvidersByGOName[goName].AddRange(newLocomotionProviders);
                }
            }
        }

        float GetElementHeight(int index)
        {
            // ReorderableList will still invoke this callback when List is Empty,
            // so return the default element height as a fallback (even though this value isn't used)
            // to avoid indexing out of bounds.
            if (index >= m_LocomotionVignetteProviders.arraySize)
                return m_EditorProviderList.elementHeight;

            var element = m_LocomotionVignetteProviders.GetArrayElementAtIndex(index);
            var enabled = element.FindPropertyRelative("m_Enabled");
            var useOverrideParameters = element.FindPropertyRelative("m_OverrideDefaultParameters");

            if (!s_HeightsMeasured)
            {
                var locomotionProviderProp = element.FindPropertyRelative("m_LocomotionProvider");

                // Get SerializedProperties of the provider's override parameters.
                var parameterProperties = element.FindPropertyRelative("m_OverrideParameters");

                var parameters = new VignetteParameterProperties(parameterProperties);

                // Return the element height depending on whether the locomotion provider's parameters needs to be override.
                s_UnexpandedHeight = k_Padding
                    + EditorGUI.GetPropertyHeight(locomotionProviderProp) + k_Padding
                    + EditorGUI.GetPropertyHeight(enabled) + k_Padding
                    + EditorGUI.GetPropertyHeight(useOverrideParameters) + k_Padding;

                s_ExpandedHeight = s_UnexpandedHeight
                    + EditorGUI.GetPropertyHeight(parameters.apertureSize) + k_Padding
                    + EditorGUI.GetPropertyHeight(parameters.featheringEffect) + k_Padding
                    + EditorGUI.GetPropertyHeight(parameters.easeInTime) + k_Padding
                    + EditorGUI.GetPropertyHeight(parameters.easeOutTime) + k_Padding
                    + EditorGUI.GetPropertyHeight(parameters.easeInTimeLock) + k_Padding
                    + EditorGUI.GetPropertyHeight(parameters.easeOutDelayTime) + k_Padding
                    + EditorGUI.GetPropertyHeight(parameters.vignetteColor) + k_Padding
                    + EditorGUI.GetPropertyHeight(parameters.vignetteColorBlend) + k_Padding
                    + EditorGUI.GetPropertyHeight(parameters.apertureVerticalPosition) + k_Padding;

                s_HeightsMeasured = true;
            }

            return useOverrideParameters.boolValue ? s_ExpandedHeight : s_UnexpandedHeight;
        }

        void DrawCurrentParameters()
        {
            // Layout Default Parameters.
            m_CurrentParameters.isExpanded =
                EditorGUILayout.BeginFoldoutHeaderGroup(m_CurrentParameters.isExpanded, Contents.currentParameters);

            if (m_CurrentParameters.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(m_CurrentParameterProperties.apertureSize, Contents.apertureSize);
                        EditorGUILayout.PropertyField(m_CurrentParameterProperties.featheringEffect, Contents.featheringEffect);
                        EditorGUILayout.PropertyField(m_CurrentParameterProperties.easeInTime, Contents.easeInTime);
                        EditorGUILayout.PropertyField(m_CurrentParameterProperties.easeOutTime, Contents.easeOutTime);
                        EditorGUILayout.PropertyField(m_CurrentParameterProperties.easeInTimeLock, Contents.easeInTimeLock);
                        EditorGUILayout.PropertyField(m_CurrentParameterProperties.easeOutDelayTime, Contents.easeOutDelayTime);
                        EditorGUILayout.PropertyField(m_CurrentParameterProperties.vignetteColor, Contents.vignetteColor);
                        EditorGUILayout.PropertyField(m_CurrentParameterProperties.vignetteColorBlend, Contents.vignetteColorBlend);
                        EditorGUILayout.PropertyField(m_CurrentParameterProperties.apertureVerticalPosition, Contents.apertureVerticalPosition);
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawDefaultParameters()
        {
            // Layout Default Parameters.
            m_DefaultParameters.isExpanded =
                EditorGUILayout.BeginFoldoutHeaderGroup(m_DefaultParameters.isExpanded, Contents.defaultParameters);

            if (m_DefaultParameters.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_DefaultParameterProperties.apertureSize, Contents.apertureSize);
                    EditorGUILayout.PropertyField(m_DefaultParameterProperties.featheringEffect, Contents.featheringEffect);
                    EditorGUILayout.PropertyField(m_DefaultParameterProperties.easeInTime, Contents.easeInTime);
                    EditorGUILayout.PropertyField(m_DefaultParameterProperties.easeOutTime, Contents.easeOutTime);
                    EditorGUILayout.PropertyField(m_DefaultParameterProperties.easeInTimeLock, Contents.easeInTimeLock);
                    EditorGUILayout.PropertyField(m_DefaultParameterProperties.easeOutDelayTime, Contents.easeOutDelayTime);
                    EditorGUILayout.PropertyField(m_DefaultParameterProperties.vignetteColor, Contents.vignetteColor);
                    EditorGUILayout.PropertyField(m_DefaultParameterProperties.vignetteColorBlend, Contents.vignetteColorBlend);
                    EditorGUILayout.PropertyField(m_DefaultParameterProperties.apertureVerticalPosition, Contents.apertureVerticalPosition);

                    EditorGUILayout.Space();

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Space(indent);
                            if (GUILayout.Button(Contents.resetToDefaults))
                                m_TunnelingVignetteController.defaultParameters.CopyFrom(VignetteParameters.Defaults.defaultEffect);
                        }

                        if (check.changed)
                            GUI.FocusControl(null);
                    }

                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawPreviewDropDownMenu()
        {
            if (Application.isPlaying)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.LabelField(Contents.previewInEditorDisabled);
                }
            }
            else
            {
                // Update the locomotion provider names in the preview menu before drawing.
                UpdatePreviewMenu();

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    // Update dropdown menu display after selecting one of its options.
                    m_PreviewIndex = EditorGUILayout.Popup(Contents.previewInEditor, m_PreviewIndex, m_PreviewList.ToArray());

                    if (check.changed)
                    {
                        // This is needed for Unity versions before 2020 LTS to trigger a change of the preview in the editor without any delay or the need of having the mouse hover the game view,
                        // because the UpdateTunnelingVignette method in the controller class uses MaterialPropertyBlock that does not automatically trigger the editor change.
                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                }
            }
        }

        void DrawLocomotionVignetteProviders()
        {
            // Layout the list header.
            m_LocomotionVignetteProviders.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_LocomotionVignetteProviders.isExpanded, Contents.locomotionVignetteProviders);

            // Layout the expanded Providers. See the DrawListElements() callback method for details.
            if (m_LocomotionVignetteProviders.isExpanded)
            {
                EditorGUILayout.Space(k_Padding);
                m_EditorProviderList.DoLayoutList();
            }

            EditorGUI.EndFoldoutHeaderGroup();
        }

        void EditorPreview()
        {
            // Update editor preview.
            switch (m_PreviewIndex)
            {
                case 0: // Preview No Effect
                    m_TunnelingVignetteController.PreviewInEditor(VignetteParameters.Defaults.noEffect);
                    break;

                case 1: // Preview Default Parameters
                    m_TunnelingVignetteController.PreviewInEditor(m_TunnelingVignetteController.defaultParameters);
                    break;

                case var i when (i > 1): // Preview Locomotion Vignette Providers

                    if (m_ControllerLocomotionVignetteProviders != null && m_ControllerLocomotionVignetteProviders?.Count > 0)
                        m_TunnelingVignetteController.PreviewInEditor(m_ControllerLocomotionVignetteProviders[i - 2].overrideDefaultParameters
                                    ? m_ControllerLocomotionVignetteProviders[i - 2].vignetteParameters
                                    : m_TunnelingVignetteController.defaultParameters);
                    else
                        m_TunnelingVignetteController.PreviewInEditor(VignetteParameters.Defaults.noEffect);

                    break;
            }
        }

        /// <inheritdoc />
        protected override void DrawInspector()
        {
            base.DrawScript();

            DrawDefaultParameters();

            EditorGUILayout.Space();

            DrawPreviewDropDownMenu();

            EditorGUILayout.Space();

            DrawLocomotionVignetteProviders();

            EditorGUILayout.Space();

            if (!Application.isPlaying)
                EditorPreview();
            else
                DrawCurrentParameters();
        }
    }
}
